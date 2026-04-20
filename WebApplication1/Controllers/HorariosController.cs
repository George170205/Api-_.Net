using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Horario semanal recurrente (PDF §2.1, §2.2).
    /// Se construye desde la tabla <c>Horario</c> (definición por día de semana),
    /// uniendo Grupo → Materia → Docente.
    ///
    /// Rutas:
    ///   GET /api/horarios/estudiante/{estudianteId}
    ///     → todos los horarios de las materias en que está inscrito.
    ///   GET /api/horarios/docente/{docenteId}
    ///     → todos los horarios de los grupos a cargo del docente.
    ///
    /// Resultado ordenado por día de semana + hora de inicio para que el
    /// cliente pueda renderizar directo sin ordenar.
    /// </summary>
    [ApiController]
    [Route("api/horarios")]
    [Authorize]
    public class HorariosController : ControllerBase
    {
        private static readonly string[] _diasNombre =
        {
            "", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"
        };

        private readonly AppDbContext _context;
        public HorariosController(AppDbContext context) => _context = context;

        [HttpGet("estudiante/{estudianteId:int}")]
        public async Task<ActionResult<IEnumerable<HorarioDto>>> GetByEstudiante(int estudianteId)
        {
            var grupoIDs = await _context.Inscripciones
                .AsNoTracking()
                .Where(i => i.EstudianteID == estudianteId &&
                            (i.Estado == null || i.Estado == "Activa" || i.Estado == "Cursando"))
                .Select(i => i.GrupoID)
                .ToListAsync();

            if (grupoIDs.Count == 0) return Ok(Array.Empty<HorarioDto>());

            var horarios = await BuildHorariosQuery(grupoIDs).ToListAsync();
            return Ok(horarios);
        }

        [HttpGet("docente/{docenteId:int}")]
        public async Task<ActionResult<IEnumerable<HorarioDto>>> GetByDocente(int docenteId)
        {
            var grupoIDs = await _context.Grupos
                .AsNoTracking()
                .Where(g => g.DocenteID == docenteId)
                .Select(g => g.GrupoID)
                .ToListAsync();

            if (grupoIDs.Count == 0) return Ok(Array.Empty<HorarioDto>());

            var horarios = await BuildHorariosQuery(grupoIDs).ToListAsync();
            return Ok(horarios);
        }

        /// <summary>
        /// Query compartida por las dos rutas — encapsula el join y la selección
        /// de columnas en un solo lugar para que cualquier cambio de esquema
        /// (p. ej. renombre de Aula) se refleje en ambas sin duplicar código.
        /// </summary>
        private IQueryable<HorarioDto> BuildHorariosQuery(List<int> grupoIDs)
        {
            return _context.Horarios
                .AsNoTracking()
                .Where(h => grupoIDs.Contains(h.GrupoID))
                .Include(h => h.Grupo).ThenInclude(g => g.Materia)
                .Include(h => h.Grupo).ThenInclude(g => g.Docente).ThenInclude(d => d.Usuario)
                .OrderBy(h => h.DiaSemana).ThenBy(h => h.HoraInicio)
                .Select(h => new HorarioDto
                {
                    HorarioID     = h.HorarioID,
                    GrupoID       = h.GrupoID,
                    MateriaID     = h.Grupo.MateriaID,
                    DocenteID     = h.Grupo.DocenteID,
                    DiaSemana     = h.DiaSemana,
                    DiaNombre     = h.DiaSemana >= 1 && h.DiaSemana <= 7 ? _diasNombre[h.DiaSemana] : "?",
                    HoraInicio    = h.HoraInicio.ToString(@"hh\:mm"),
                    HoraFin       = h.HoraFin.ToString(@"hh\:mm"),
                    MateriaNombre = h.Grupo.Materia.NombreMateria,
                    MateriaCodigo = h.Grupo.Materia.CodigoMateria,
                    GrupoCodigo   = h.Grupo.CodigoGrupo,
                    Docente       = h.Grupo.Docente.Usuario.Nombre + " " + h.Grupo.Docente.Usuario.Apellido,
                    Aula          = h.Aula
                });
        }
    }
}
