using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using WebApplication1.Infrastructure.UnitOfWork;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Generación y validación de códigos QR para registro de asistencia.
    ///
    /// PDF §3.3 "Flujo de Operación de Asistencias":
    ///   1. Docente inicia sesión de clase y genera QR (token único + imagen PNG).
    ///   2. Estudiante escanea y el backend valida (token, expiración, inscripción, duplicados).
    ///   3. Se genera un nuevo QR cada N minutos; los anteriores quedan invalidados.
    ///
    /// Rutas:
    ///   POST /api/qr/generar   (Docente, Administrador)
    ///   POST /api/qr/validar   (Autenticado)
    ///   GET  /api/qr/{token}   (consulta estado)
    ///
    /// Usa Unit of Work (PDF §3.2) para commit transaccional de invalidación de
    /// QRs previos + creación del nuevo QR, y para el par validación+alta de
    /// asistencia (atómico).
    /// </summary>
    [ApiController]
    [Route("api/qr")]
    public class QrController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        public QrController(IUnitOfWork uow) => _uow = uow;

        public record GenerarRequest(int SesionClaseID, int? DuracionMinutos);
        public record ValidarRequest(string Token, int EstudianteID, decimal? Latitud, decimal? Longitud);

        // =====================================================
        // POST /api/qr/generar   (Docente, Administrador)
        // =====================================================
        [HttpPost("generar")]
        [Authorize(Roles = "Docente,Administrador")]
        public async Task<IActionResult> Generar([FromBody] GenerarRequest req)
        {
            var sesion = await _uow.SesionesClase.GetByIdAsync(req.SesionClaseID);
            if (sesion == null) return NotFound(new { message = "Sesión de clase no encontrada" });

            await _uow.BeginTransactionAsync();
            try
            {
                // Invalidar QRs previos (PDF §3.3: "los anteriores se invalidan")
                var qrsPrevios = await _uow.QRsGenerados
                    .FindAsync(q => q.SesionClaseID == req.SesionClaseID && q.Activo == true);
                foreach (var qr in qrsPrevios)
                {
                    qr.Activo = false;
                    _uow.QRsGenerados.Update(qr);
                }

                var duracion = req.DuracionMinutos is > 0 and <= 15 ? req.DuracionMinutos.Value : 5;

                var nuevoQR = new QRGenerado
                {
                    SesionClaseID = req.SesionClaseID,
                    TokenUnico = Guid.NewGuid().ToString("N"),
                    FechaGeneracion = DateTime.UtcNow,
                    FechaExpiracion = DateTime.UtcNow.AddMinutes(duracion),
                    Activo = true
                };
                await _uow.QRsGenerados.AddAsync(nuevoQR);
                await _uow.CommitTransactionAsync();

                // Generar imagen PNG del QR (base64) — PDF §4 stack: QRCoder
                var qrImagenBase64 = GenerarQrPngBase64(nuevoQR.TokenUnico);

                return Ok(new
                {
                    qrGeneradoID = nuevoQR.QRGeneradoID,
                    token = nuevoQR.TokenUnico,
                    expiraEn = nuevoQR.FechaExpiracion,
                    duracionMinutos = duracion,
                    imagenBase64 = qrImagenBase64 // data URI lista para Image.Source en MAUI
                });
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
        }

        // =====================================================
        // POST /api/qr/validar   (Autenticado)
        // =====================================================
        [HttpPost("validar")]
        [Authorize]
        public async Task<IActionResult> Validar([FromBody] ValidarRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Token))
                return BadRequest(new { message = "Token requerido" });

            // Acceso directo al IQueryable para Include — el repositorio genérico lo permite
            var qr = await _uow.QRsGenerados.Query()
                .Include(q => q.SesionClase)
                .FirstOrDefaultAsync(q => q.TokenUnico == req.Token);

            if (qr == null) return NotFound(new { message = "QR no encontrado" });
            if (qr.Activo != true) return BadRequest(new { message = "QR inactivo" });
            if (qr.FechaExpiracion < DateTime.UtcNow) return BadRequest(new { message = "QR expirado" });

            // Verificar inscripción en el grupo de la sesión
            var inscrito = await _uow.Inscripciones.AnyAsync(i =>
                i.EstudianteID == req.EstudianteID && i.GrupoID == qr.SesionClase.GrupoID);
            if (!inscrito) return Forbid();

            // Evitar duplicados
            var yaRegistrada = await _uow.Asistencias.AnyAsync(a =>
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
            await _uow.Asistencias.AddAsync(asistencia);
            await _uow.SaveChangesAsync();

            return Ok(new { message = "Asistencia registrada", asistenciaID = asistencia.AsistenciaID });
        }

        // =====================================================
        // GET /api/qr/{token}
        // =====================================================
        [HttpGet("{token}")]
        public async Task<IActionResult> GetByToken(string token)
        {
            var qr = await _uow.QRsGenerados.FirstOrDefaultAsync(q => q.TokenUnico == token);
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

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------
        private static string GenerarQrPngBase64(string token)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(token, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            var bytes = qrCode.GetGraphic(20);
            return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        }
    }
}
