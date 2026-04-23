using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.DTOs;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Perfil y estadísticas agregadas del alumno (PDF §2.1).
    ///
    /// Ruta principal:
    ///   GET /api/estudiantes/{id}/home
    ///     → nombre, matrícula, carrera, # materias activas, promedio global,
    ///       % asistencia acumulado, próxima clase del día y la lista de
    ///       materias en las que está inscrito actualmente.
    ///
    /// Se consolida en un solo endpoint para que la UI no dispare 3–4 requests
    /// al abrir la pantalla Home.
    /// </summary>
    [ApiController]
    [Route("api/estudiantes")]
    [Authorize]
    public class EstudiantesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public EstudiantesController(AppDbContext context) => _context = context;

        [HttpGet("{id:int}/home")]
        public async Task<ActionResult<HomeAlumnoDto>> GetHome(int id)
        {
            if (!User.IsInRole("Administrador") && !User.IsInRole("Docente"))
            {
                var estudianteAutenticadoId = await GetAuthenticatedEstudianteIdAsync();
                if (!estudianteAutenticadoId.HasValue) return Forbid();
                if (estudianteAutenticadoId.Value != id) return Forbid();
            }

            var estudiante = await _context.Estudiantes
                .AsNoTracking()
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e => e.EstudianteID == id);

            if (estudiante == null) return NotFound();

            var nombre    = estudiante.Usuario?.Nombre ?? string.Empty;
            var apellido  = estudiante.Usuario?.Apellido ?? string.Empty;
            var completo  = $"{nombre} {apellido}".Trim();
            var iniciales = GetIniciales(nombre, apellido);

            // Inscripciones activas (Estado == "Activa" o null por compatibilidad).
            var inscripciones = await _context.Inscripciones
                .AsNoTracking()
                .Where(i => i.EstudianteID == id &&
                            (i.Estado == null || i.Estado == "Activa" || i.Estado == "Cursando"))
                .Include(i => i.Grupo).ThenInclude(g => g.Materia)
                .Include(i => i.Grupo).ThenInclude(g => g.Docente).ThenInclude(d => d.Usuario)
                .ToListAsync();

            var grupoIDs = inscripciones.Select(i => i.GrupoID).ToList();

            // Conteo de sesiones + asistencias para calcular % global y por materia.
            var sesiones = await _context.SesionesClase
                .AsNoTracking()
                .Where(s => grupoIDs.Contains(s.GrupoID))
                .Select(s => new { s.SesionClaseID, s.GrupoID, s.Fecha, s.HoraInicio, s.HoraFin, s.Aula, s.Tema })
                .ToListAsync();

            var asistencias = await _context.Asistencias
                .AsNoTracking()
                .Where(a => a.EstudianteID == id)
                .Select(a => new { a.SesionClaseID, a.Estado })
                .ToListAsync();

            var asistSet = asistencias
                .Where(a => a.Estado == "Presente" || a.Estado == "Retardo")
                .Select(a => a.SesionClaseID)
                .ToHashSet();

            int totalSesiones  = sesiones.Count;
            int totalAsistidas = sesiones.Count(s => asistSet.Contains(s.SesionClaseID));
            int faltas         = totalSesiones - totalAsistidas;
            int porcentajeGlobal = totalSesiones > 0
                ? (int)Math.Round(100.0 * totalAsistidas / totalSesiones)
                : 0;

            // Promedio global: promedio simple de CalificacionFinal de inscripciones finalizadas.
            var finales = inscripciones
                .Where(i => i.CalificacionFinal.HasValue)
                .Select(i => i.CalificacionFinal!.Value)
                .ToList();
            decimal promedio = finales.Count > 0 ? Math.Round(finales.Average(), 1) : 0m;

            // Materias activas (tarjetas).
            var materias = inscripciones.Select(i =>
            {
                var sesGrupo   = sesiones.Where(s => s.GrupoID == i.GrupoID).Count();
                var asistGrupo = sesiones.Where(s => s.GrupoID == i.GrupoID && asistSet.Contains(s.SesionClaseID)).Count();
                int pct = sesGrupo > 0 ? (int)Math.Round(100.0 * asistGrupo / sesGrupo) : 0;
                var prof = i.Grupo?.Docente?.Usuario != null
                    ? $"{i.Grupo.Docente.Usuario.Nombre} {i.Grupo.Docente.Usuario.Apellido}".Trim()
                    : null;
                return new MateriaActivaDto
                {
                    MateriaID  = i.Grupo?.MateriaID ?? 0,
                    GrupoID    = i.GrupoID,
                    Nombre     = i.Grupo?.Materia?.NombreMateria ?? "—",
                    Codigo     = i.Grupo?.Materia?.CodigoMateria,
                    Profesor   = prof,
                    Salon      = null,
                    Horario    = null,
                    Porcentaje = pct
                };
            }).ToList();

            // Próxima sesión: la más cercana en el futuro (o hoy con hora fin no vencida).
            var ahora = DateTime.Now;
            var proxima = sesiones
                .Where(s => s.Fecha.Date > ahora.Date
                         || (s.Fecha.Date == ahora.Date && s.HoraFin >= ahora.TimeOfDay))
                .OrderBy(s => s.Fecha).ThenBy(s => s.HoraInicio)
                .FirstOrDefault();

            ProximaClaseDto? proximaDto = null;
            if (proxima != null)
            {
                var inscr = inscripciones.FirstOrDefault(i => i.GrupoID == proxima.GrupoID);
                var profe = inscr?.Grupo?.Docente?.Usuario != null
                    ? $"{inscr.Grupo.Docente.Usuario.Nombre} {inscr.Grupo.Docente.Usuario.Apellido}".Trim()
                    : null;
                proximaDto = new ProximaClaseDto
                {
                    SesionClaseID = proxima.SesionClaseID,
                    GrupoID       = proxima.GrupoID,
                    Nombre        = inscr?.Grupo?.Materia?.NombreMateria ?? "—",
                    Salon         = proxima.Aula,
                    Fecha         = proxima.Fecha,
                    HoraInicio    = proxima.HoraInicio.ToString(@"hh\:mm"),
                    HoraFin       = proxima.HoraFin.ToString(@"hh\:mm"),
                    Profesor      = profe
                };
            }

            var dto = new HomeAlumnoDto
            {
                EstudianteID         = estudiante.EstudianteID,
                NombreCompleto       = completo,
                Iniciales            = iniciales,
                Matricula            = estudiante.Matricula ?? string.Empty,
                Carrera              = estudiante.Carrera,
                Semestre             = estudiante.Semestre,
                Email                = estudiante.Usuario?.Email,
                MateriasActivas      = materias.Count,
                Promedio             = promedio,
                PorcentajeAsistencia = porcentajeGlobal,
                FaltasTotales        = faltas,
                ProximaClase         = proximaDto,
                MateriasList         = materias
            };

            return Ok(dto);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Estudiante")]
        public async Task<ActionResult<HomeAlumnoDto>> GetMyHome()
        {
            var estudianteAutenticadoId = await GetAuthenticatedEstudianteIdAsync();
            if (!estudianteAutenticadoId.HasValue) return Forbid();
            return await GetHome(estudianteAutenticadoId.Value);
        }

        private static string GetIniciales(string nombre, string apellido)
        {
            char n = !string.IsNullOrWhiteSpace(nombre)   ? char.ToUpper(nombre[0])   : '?';
            char a = !string.IsNullOrWhiteSpace(apellido) ? char.ToUpper(apellido[0]) : '?';
            return $"{n}{a}";
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
