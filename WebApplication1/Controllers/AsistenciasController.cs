using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Consulta y registro de Asistencias.
    ///
    /// PDF §2.1 "Perfil Estudiante": historial cronológico de asistencias.
    /// PDF §2.2 "Perfil Docente": vista detallada de asistencias por materia y grupo.
    /// PDF §5.1 "Entidades Principales": Asistencia.
    ///
    /// Rutas:
    ///   GET    /api/asistencias
    ///   GET    /api/asistencias/estudiante/{estudianteId}
    ///   GET    /api/asistencias/sesion/{sesionClaseId}
    ///   POST   /api/asistencias            (registro manual por docente)
    ///   PUT    /api/asistencias/{id}/justificar
    /// </summary>
    [ApiController]
    [Route("api/asistencias")]
    public class AsistenciasController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AsistenciasController(AppDbContext context) => _context = context;

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<Asistencia>>> Get()
            => await _context.Asistencias
                .OrderByDescending(a => a.FechaRegistro)
                .AsNoTracking()
                .ToListAsync();

        [HttpGet("estudiante/{estudianteId:int}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Asistencia>>> GetPorEstudiante(int estudianteId)
        {
            if (!User.IsInRole("Administrador") && !User.IsInRole("Docente"))
            {
                var estudianteAutenticadoId = await GetAuthenticatedEstudianteIdAsync();
                if (!estudianteAutenticadoId.HasValue) return Forbid();
                if (estudianteAutenticadoId.Value != estudianteId) return Forbid();
            }

            var asistencias = await _context.Asistencias
                .Where(a => a.EstudianteID == estudianteId)
                .Include(a => a.SesionClase)
                .OrderByDescending(a => a.FechaRegistro)
                .AsNoTracking()
                .ToListAsync();
            return asistencias;
        }

        [HttpGet("sesion/{sesionClaseId:int}")]
        public async Task<ActionResult<IEnumerable<Asistencia>>> GetPorSesion(int sesionClaseId)
            => await _context.Asistencias
                .Where(a => a.SesionClaseID == sesionClaseId)
                .Include(a => a.Estudiante)
                .AsNoTracking()
                .ToListAsync();

        /// <summary>
        /// Próximas sesiones de clase del alumno (PDF §3.3).
        /// Toma las sesiones pertenecientes a los grupos donde el alumno está
        /// inscrito, filtra las que aún no han terminado (hoy con HoraFin
        /// pendiente, o fechas futuras) y marca las que ya tienen asistencia
        /// registrada para este estudiante.
        ///
        /// Query:
        ///   limit — número máximo de sesiones a devolver (default 10, clamp 1..50).
        /// </summary>
        [HttpGet("estudiante/{estudianteId:int}/proximas")]
        public async Task<ActionResult<IEnumerable<ProximaAsistenciaDto>>> GetProximas(
            int estudianteId, [FromQuery] int limit = 10)
        {
            limit = Math.Clamp(limit, 1, 50);

            var grupoIDs = await _context.Inscripciones
                .AsNoTracking()
                .Where(i => i.EstudianteID == estudianteId &&
                            (i.Estado == null || i.Estado == "Activa" || i.Estado == "Cursando"))
                .Select(i => i.GrupoID)
                .ToListAsync();

            if (grupoIDs.Count == 0) return Ok(Array.Empty<ProximaAsistenciaDto>());

            var ahora = DateTime.Now;
            var fechaHoy = ahora.Date;
            var horaActual = ahora.TimeOfDay;

            // Se materializa la consulta básica primero para evitar que el
            // filtro de "hoy con hora fin >= ahora" explote en SQL Server /
            // Npgsql con mezcla de DateTime + TimeSpan.
            var sesiones = await _context.SesionesClase
                .AsNoTracking()
                .Where(s => grupoIDs.Contains(s.GrupoID) && s.Fecha.Date >= fechaHoy)
                .Include(s => s.Grupo).ThenInclude(g => g.Materia)
                .Include(s => s.Grupo).ThenInclude(g => g.Docente).ThenInclude(d => d.Usuario)
                .OrderBy(s => s.Fecha).ThenBy(s => s.HoraInicio)
                .Take(limit * 3) // margen; filtramos HoraFin en memoria
                .ToListAsync();

            var sesionesValidas = sesiones
                .Where(s => s.Fecha.Date > fechaHoy ||
                            (s.Fecha.Date == fechaHoy && s.HoraFin >= horaActual))
                .Take(limit)
                .ToList();

            var sesionIDs = sesionesValidas.Select(s => s.SesionClaseID).ToList();
            var asistenciasExistentes = await _context.Asistencias
                .AsNoTracking()
                .Where(a => a.EstudianteID == estudianteId && sesionIDs.Contains(a.SesionClaseID))
                .ToDictionaryAsync(a => a.SesionClaseID, a => a.Estado);

            var result = sesionesValidas.Select(s =>
            {
                var profesor = s.Grupo?.Docente?.Usuario != null
                    ? $"{s.Grupo.Docente.Usuario.Nombre} {s.Grupo.Docente.Usuario.Apellido}".Trim()
                    : null;
                asistenciasExistentes.TryGetValue(s.SesionClaseID, out var estado);
                return new ProximaAsistenciaDto
                {
                    SesionClaseID    = s.SesionClaseID,
                    GrupoID          = s.GrupoID,
                    MateriaID        = s.Grupo?.MateriaID ?? 0,
                    MateriaNombre    = s.Grupo?.Materia?.NombreMateria ?? "—",
                    Profesor         = profesor,
                    Fecha            = s.Fecha,
                    HoraInicio       = s.HoraInicio.ToString(@"hh\:mm"),
                    HoraFin          = s.HoraFin.ToString(@"hh\:mm"),
                    Aula             = s.Aula,
                    Tema             = s.Tema,
                    YaRegistrada     = estado != null,
                    EstadoAsistencia = estado
                };
            }).ToList();

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Docente,Administrador")]
        public async Task<ActionResult<Asistencia>> Create(Asistencia asistencia)
        {
            asistencia.FechaRegistro ??= DateTime.UtcNow;
            _context.Asistencias.Add(asistencia);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPorEstudiante),
                new { estudianteId = asistencia.EstudianteID }, asistencia);
        }

        public record JustificarRequest(string? Observaciones);

        [HttpPut("{id:int}/justificar")]
        [Authorize(Roles = "Docente,Administrador")]
        public async Task<IActionResult> Justificar(int id, [FromBody] JustificarRequest req)
        {
            var asistencia = await _context.Asistencias.FindAsync(id);
            if (asistencia == null) return NotFound();

            asistencia.Estado = "Justificada";
            asistencia.Observaciones = req.Observaciones;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<int?> GetAuthenticatedEstudianteIdAsync()
        {
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(usuarioIdClaim, out var usuarioId)) return null;

            return await _context.Estudiantes
                .AsNoTracking()
                .Where(e => e.UsuarioID == usuarioId)
                .Select(e => (int?)e.EstudianteID)
                .FirstOrDefaultAsync();
        }
    }
}
