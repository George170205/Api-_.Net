using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Generación y validación de códigos QR para registro de asistencia.
    ///
    /// PDF §3.3 "Flujo de Operación de Asistencias":
    ///   1. Docente inicia sesión de clase y genera QR.
    ///   2. Estudiante escanea y el backend valida (token, expiración, inscripción, duplicados).
    ///
    /// Rutas:
    ///   POST /api/qr/generar   (Docente)
    ///   POST /api/qr/validar   (Estudiante)
    ///   GET  /api/qr/{token}   (consulta estado)
    /// </summary>
    [ApiController]
    [Route("api/qr")]
    public class QrController : ControllerBase
    {
        private readonly AppDbContext _context;
        public QrController(AppDbContext context) => _context = context;

        public record GenerarRequest(int SesionClaseID, int? DuracionMinutos);
        public record ValidarRequest(string Token, int EstudianteID, decimal? Latitud, decimal? Longitud);

        // =====================================================
        // POST /api/qr/generar   (Docente)
        // =====================================================
        [HttpPost("generar")]
        [Authorize(Roles = "Docente,Administrador")]
        public async Task<IActionResult> Generar([FromBody] GenerarRequest req)
        {
            var sesion = await _context.SesionesClase.FindAsync(req.SesionClaseID);
            if (sesion == null) return NotFound(new { message = "Sesión de clase no encontrada" });

            // Invalidar QRs previos (§3.3: "los anteriores se invalidan")
            var qrsPrevios = _context.QRsGenerados.Where(q => q.SesionClaseID == req.SesionClaseID && q.Activo == true);
            foreach (var qr in qrsPrevios) qr.Activo = false;

            var duracion = req.DuracionMinutos is > 0 and <= 15 ? req.DuracionMinutos.Value : 5;

            var nuevoQR = new QRGenerado
            {
                SesionClaseID = req.SesionClaseID,
                TokenUnico = Guid.NewGuid().ToString("N"),
                FechaGeneracion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddMinutes(duracion),
                Activo = true
            };
            _context.QRsGenerados.Add(nuevoQR);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                qrGeneradoID = nuevoQR.QRGeneradoID,
                token = nuevoQR.TokenUnico,
                expiraEn = nuevoQR.FechaExpiracion,
                duracionMinutos = duracion
            });
        }

        // =====================================================
        // POST /api/qr/validar   (Estudiante)
        // =====================================================
        [HttpPost("validar")]
        [Authorize]
        public async Task<IActionResult> Validar([FromBody] ValidarRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Token))
                return BadRequest(new { message = "Token requerido" });

            var qr = await _context.QRsGenerados
                .Include(q => q.SesionClase)
                .FirstOrDefaultAsync(q => q.TokenUnico == req.Token);

            if (qr == null) return NotFound(new { message = "QR no encontrado" });
            if (qr.Activo != true) return BadRequest(new { message = "QR inactivo" });
            if (qr.FechaExpiracion < DateTime.UtcNow) return BadRequest(new { message = "QR expirado" });

            // Verificar inscripción en el grupo de la sesión
            var inscrito = await _context.Inscripciones.AnyAsync(i =>
                i.EstudianteID == req.EstudianteID && i.GrupoID == qr.SesionClase.GrupoID);
            if (!inscrito) return Forbid();

            // Evitar duplicados
            var yaRegistrada = await _context.Asistencias.AnyAsync(a =>
                a.EstudianteID == req.EstudianteID && a.SesionClaseID == qr.SesionClaseID);
            if (yaRegistrada) return Conflict(new { message = "Asistencia ya registrada" });

            var asistencia = new Asistencia
            {
                SesionClaseID = qr.SesionClaseID,
                EstudianteID = req.EstudianteID,
                QRGeneradoID = qr.QRGeneradoID,
                FechaRegistro = DateTime.UtcNow,
                Estado = "Presente",
                Latitud = req.Latitud,
                Longitud = req.Longitud
            };
            _context.Asistencias.Add(asistencia);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Asistencia registrada", asistenciaID = asistencia.AsistenciaID });
        }

        // =====================================================
        // GET /api/qr/{token}
        // =====================================================
        [HttpGet("{token}")]
        public async Task<IActionResult> GetByToken(string token)
        {
            var qr = await _context.QRsGenerados.FirstOrDefaultAsync(q => q.TokenUnico == token);
            if (qr == null) return NotFound();
            return Ok(new
            {
                qr.QRGeneradoID,
                qr.SesionClaseID,
                qr.Activo,
                qr.FechaGeneracion,
                qr.FechaExpiracion,
                expirado = qr.FechaExpiracion < DateTime.UtcNow
            });
        }
    }
}
