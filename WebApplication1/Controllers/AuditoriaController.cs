using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Bitácora consolidada del sistema (PDF §2.3 + §6).
    ///
    /// Al no existir una tabla dedicada, se arma "on the fly" desde las
    /// tablas operativas. Cuando se agregue una tabla `BitacoraAuditoria`,
    /// este controlador pasa a leer directamente de ella.
    ///
    /// Ruta:
    ///   GET /api/auditoria?tipo=Todos&desde=...&hasta=...&limit=100
    /// </summary>
    [ApiController]
    [Route("api/auditoria")]
    [Authorize(Roles = "Administrador")]
    public class AuditoriaController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AuditoriaController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventoAuditoriaDto>>> Get(
            [FromQuery] string tipo = "Todos",
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null,
            [FromQuery] int limit = 100)
        {
            limit = Math.Clamp(limit, 1, 500);
            var eventos = new List<EventoAuditoriaDto>();

            bool incluye(string t) => tipo == "Todos" || tipo.Equals(t, StringComparison.OrdinalIgnoreCase);
            bool enRango(DateTime? fecha) =>
                fecha.HasValue
                && (!desde.HasValue || fecha.Value >= desde.Value)
                && (!hasta.HasValue || fecha.Value <= hasta.Value);

            if (incluye("Login"))
            {
                var logins = await _context.IntentosLogin
                    .AsNoTracking()
                    .OrderByDescending(i => i.FechaIntento)
                    .Take(limit)
                    .ToListAsync();
                eventos.AddRange(logins.Where(i => enRango(i.FechaIntento)).Select(i => new EventoAuditoriaDto
                {
                    Tipo    = "Login",
                    Fecha   = i.FechaIntento ?? DateTime.MinValue,
                    Resumen = $"{(i.Exitoso ? "Login OK" : "Login fallido")}: {i.Email}",
                    Meta    = $"IP {i.DireccionIP ?? "?"} · {i.MotivoFallo ?? "-"}"
                }));
            }

            if (incluye("QR"))
            {
                var qrs = await _context.QRsGenerados
                    .AsNoTracking()
                    .OrderByDescending(q => q.FechaGeneracion)
                    .Take(limit)
                    .ToListAsync();
                eventos.AddRange(qrs.Where(q => enRango(q.FechaGeneracion)).Select(q => new EventoAuditoriaDto
                {
                    Tipo    = "QR",
                    Fecha   = q.FechaGeneracion ?? DateTime.MinValue,
                    Resumen = $"QR generado (SesiónID={q.SesionClaseID})",
                    Meta    = $"Expira {q.FechaExpiracion:yyyy-MM-dd HH:mm}"
                }));
            }

            if (incluye("Asistencia"))
            {
                var asist = await _context.Asistencias
                    .AsNoTracking()
                    .OrderByDescending(a => a.FechaRegistro)
                    .Take(limit)
                    .ToListAsync();
                eventos.AddRange(asist.Where(a => enRango(a.FechaRegistro)).Select(a => new EventoAuditoriaDto
                {
                    Tipo    = "Asistencia",
                    Fecha   = a.FechaRegistro ?? DateTime.MinValue,
                    Resumen = $"Asistencia {a.Estado ?? "Registrada"} · EstudianteID={a.EstudianteID}",
                    Meta    = $"SesiónID={a.SesionClaseID}"
                }));
            }

            if (incluye("Calificacion"))
            {
                var cals = await _context.Calificaciones
                    .AsNoTracking()
                    .OrderByDescending(c => c.Fecha)
                    .Take(limit)
                    .ToListAsync();
                eventos.AddRange(cals.Where(c => enRango(c.Fecha)).Select(c => new EventoAuditoriaDto
                {
                    Tipo    = "Calificacion",
                    Fecha   = c.Fecha ?? DateTime.MinValue,
                    Resumen = $"Calificación #{c.CalificacionID} ({c.TipoEvaluacion})",
                    Meta    = $"Puntos {c.Puntos}/{c.PuntosMaximos}"
                }));
            }

            return eventos
                .OrderByDescending(e => e.Fecha)
                .Take(limit)
                .ToList();
        }
    }
}
