using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
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
        public async Task<ActionResult<IEnumerable<Asistencia>>> Get()
            => await _context.Asistencias
                .OrderByDescending(a => a.FechaRegistro)
                .AsNoTracking()
                .ToListAsync();

        [HttpGet("estudiante/{estudianteId:int}")]
        public async Task<ActionResult<IEnumerable<Asistencia>>> GetPorEstudiante(int estudianteId)
            => await _context.Asistencias
                .Where(a => a.EstudianteID == estudianteId)
                .Include(a => a.SesionClase)
                .OrderByDescending(a => a.FechaRegistro)
                .AsNoTracking()
                .ToListAsync();

        [HttpGet("sesion/{sesionClaseId:int}")]
        public async Task<ActionResult<IEnumerable<Asistencia>>> GetPorSesion(int sesionClaseId)
            => await _context.Asistencias
                .Where(a => a.SesionClaseID == sesionClaseId)
                .Include(a => a.Estudiante)
                .AsNoTracking()
                .ToListAsync();

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
    }
}
