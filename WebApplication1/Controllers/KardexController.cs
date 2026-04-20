using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Kárdex del estudiante (PDF §2.1).
    /// Construido desde Inscripcion + SesionClase + Asistencia.
    ///
    /// Ruta:
    ///   GET /api/kardex/estudiante/{estudianteId}
    /// </summary>
    [ApiController]
    [Route("api/kardex")]
    [Authorize]
    public class KardexController : ControllerBase
    {
        private readonly AppDbContext _context;
        public KardexController(AppDbContext context) => _context = context;

        [HttpGet("estudiante/{estudianteId:int}")]
        public async Task<ActionResult<IEnumerable<KardexItemDto>>> GetByEstudiante(int estudianteId)
        {
            // Inscripciones del estudiante con su grupo + materia
            var inscripciones = await _context.Inscripciones
                .AsNoTracking()
                .Where(i => i.EstudianteID == estudianteId)
                .Include(i => i.Grupo).ThenInclude(g => g.Materia)
                .ToListAsync();

            if (inscripciones.Count == 0)
                return new List<KardexItemDto>();

            var grupoIds = inscripciones.Select(i => i.GrupoID).Distinct().ToList();

            // Totales de sesiones por grupo
            var sesionesPorGrupo = await _context.SesionesClase
                .AsNoTracking()
                .Where(s => grupoIds.Contains(s.GrupoID))
                .GroupBy(s => s.GrupoID)
                .Select(g => new { GrupoID = g.Key, Total = g.Count() })
                .ToListAsync();
            var totalesMap = sesionesPorGrupo.ToDictionary(x => x.GrupoID, x => x.Total);

            // Asistencias del estudiante por grupo (joinear Asistencia → SesionClase.GrupoID)
            var asistenciasPorGrupo = await _context.Asistencias
                .AsNoTracking()
                .Where(a => a.EstudianteID == estudianteId)
                .Join(_context.SesionesClase,
                      a => a.SesionClaseID, s => s.SesionClaseID,
                      (a, s) => new { s.GrupoID, a.Estado })
                .Where(x => grupoIds.Contains(x.GrupoID))
                .GroupBy(x => x.GrupoID)
                .Select(g => new
                {
                    GrupoID = g.Key,
                    Asistidas = g.Count(x => x.Estado == "Presente" || x.Estado == "Retardo")
                })
                .ToListAsync();
            var asistidasMap = asistenciasPorGrupo.ToDictionary(x => x.GrupoID, x => x.Asistidas);

            var items = inscripciones.Select(i =>
            {
                var total     = totalesMap.GetValueOrDefault(i.GrupoID, 0);
                var asistidas = asistidasMap.GetValueOrDefault(i.GrupoID, 0);
                var pct       = total == 0 ? 0 : (int)Math.Round(asistidas * 100.0 / total);

                return new KardexItemDto
                {
                    InscripcionID        = i.InscripcionID,
                    MateriaID            = i.Grupo?.Materia?.MateriaID ?? 0,
                    Codigo               = i.Grupo?.Materia?.CodigoMateria ?? string.Empty,
                    Nombre               = i.Grupo?.Materia?.NombreMateria ?? string.Empty,
                    Ciclo                = i.Grupo?.Periodo ?? string.Empty,
                    CalificacionFinal    = i.CalificacionFinal,
                    SesionesTotales      = total,
                    SesionesAsistidas    = asistidas,
                    PorcentajeAsistencia = pct,
                    Estado               = string.IsNullOrEmpty(i.Estado) ? "Cursando" : i.Estado!
                };
            }).ToList();

            return items;
        }
    }
}
