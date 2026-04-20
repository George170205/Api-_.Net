using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Infrastructure.UnitOfWork;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Calificaciones (PDF §2.2 Docente, §2.1 Alumno).
    ///
    /// Rutas:
    ///   GET  /api/calificaciones/estudiante/{id}
    ///   GET  /api/calificaciones/grupo/{grupoId}
    ///   POST /api/calificaciones          (individual)
    ///   POST /api/calificaciones/bulk     (carga masiva por grupo)
    ///   PUT  /api/calificaciones/{id}
    /// </summary>
    [ApiController]
    [Route("api/calificaciones")]
    [Authorize]
    public class CalificacionesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _uow;

        public CalificacionesController(AppDbContext context, IUnitOfWork uow)
        {
            _context = context;
            _uow = uow;
        }

        [HttpGet("estudiante/{estudianteId:int}")]
        public async Task<ActionResult<IEnumerable<CalificacionDto>>> GetByEstudiante(int estudianteId)
        {
            var cals = await QueryBase()
                .Where(c => c.Inscripcion.EstudianteID == estudianteId)
                .ToListAsync();
            return cals.Select(ToDto).ToList();
        }

        [HttpGet("grupo/{grupoId:int}")]
        [Authorize(Roles = "Docente,Administrador")]
        public async Task<ActionResult<IEnumerable<CalificacionDto>>> GetByGrupo(int grupoId)
        {
            var cals = await QueryBase()
                .Where(c => c.Inscripcion.GrupoID == grupoId)
                .ToListAsync();
            return cals.Select(ToDto).ToList();
        }

        [HttpPost]
        [Authorize(Roles = "Docente,Administrador")]
        public async Task<ActionResult<CalificacionDto>> Create([FromBody] CalificacionUpsertDto dto)
        {
            var entity = new Calificacion
            {
                InscripcionID    = dto.InscripcionID,
                TipoEvaluacion   = dto.TipoEvaluacion,
                NumeroEvaluacion = dto.NumeroEvaluacion,
                Puntos           = dto.Puntos,
                PuntosMaximos    = dto.PuntosMaximos,
                Porcentaje       = dto.PuntosMaximos == 0 ? 0 : (dto.Puntos / dto.PuntosMaximos) * 100,
                Fecha            = DateTime.UtcNow,
                Observaciones    = dto.Observaciones
            };
            await _uow.Calificaciones.AddAsync(entity);
            await _uow.SaveChangesAsync();

            var reloaded = await QueryBase().FirstAsync(c => c.CalificacionID == entity.CalificacionID);
            return CreatedAtAction(nameof(GetByEstudiante),
                new { estudianteId = reloaded.Inscripcion.EstudianteID },
                ToDto(reloaded));
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Docente,Administrador")]
        public async Task<IActionResult> Bulk([FromBody] CalificacionBulkDto bulk)
        {
            if (bulk == null || bulk.Items == null || bulk.Items.Count == 0)
                return BadRequest(new { message = "Lista vacía" });

            await _uow.BeginTransactionAsync();
            try
            {
                foreach (var item in bulk.Items)
                {
                    var entity = new Calificacion
                    {
                        InscripcionID    = item.InscripcionID,
                        TipoEvaluacion   = bulk.TipoEvaluacion,
                        NumeroEvaluacion = bulk.NumeroEvaluacion,
                        Puntos           = item.Puntos,
                        PuntosMaximos    = bulk.PuntosMaximos,
                        Porcentaje       = bulk.PuntosMaximos == 0 ? 0 : (item.Puntos / bulk.PuntosMaximos) * 100,
                        Fecha            = DateTime.UtcNow
                    };
                    await _uow.Calificaciones.AddAsync(entity);
                }
                await _uow.CommitTransactionAsync();
                return Ok(new { insertadas = bulk.Items.Count });
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Docente,Administrador")]
        public async Task<IActionResult> Update(int id, [FromBody] CalificacionUpsertDto dto)
        {
            var cal = await _uow.Calificaciones.GetByIdAsync(id);
            if (cal == null) return NotFound();

            cal.TipoEvaluacion   = dto.TipoEvaluacion;
            cal.NumeroEvaluacion = dto.NumeroEvaluacion;
            cal.Puntos           = dto.Puntos;
            cal.PuntosMaximos    = dto.PuntosMaximos;
            cal.Porcentaje       = dto.PuntosMaximos == 0 ? 0 : (dto.Puntos / dto.PuntosMaximos) * 100;
            cal.Observaciones    = dto.Observaciones;

            _uow.Calificaciones.Update(cal);
            await _uow.SaveChangesAsync();
            return NoContent();
        }

        // ---------------------------------------------------------------
        private IQueryable<Calificacion> QueryBase() =>
            _context.Calificaciones
                .AsNoTracking()
                .Include(c => c.Inscripcion).ThenInclude(i => i.Estudiante).ThenInclude(e => e.Usuario)
                .Include(c => c.Inscripcion).ThenInclude(i => i.Grupo).ThenInclude(g => g.Materia);

        private static CalificacionDto ToDto(Calificacion c) => new()
        {
            CalificacionID     = c.CalificacionID,
            InscripcionID      = c.InscripcionID,
            EstudianteID       = c.Inscripcion?.EstudianteID ?? 0,
            EstudianteNombre   = c.Inscripcion?.Estudiante?.Usuario == null
                                    ? string.Empty
                                    : $"{c.Inscripcion.Estudiante.Usuario.Nombre} {c.Inscripcion.Estudiante.Usuario.Apellido}".Trim(),
            Matricula          = c.Inscripcion?.Estudiante?.Matricula ?? string.Empty,
            MateriaID          = c.Inscripcion?.Grupo?.MateriaID ?? 0,
            Materia            = c.Inscripcion?.Grupo?.Materia?.NombreMateria ?? string.Empty,
            TipoEvaluacion     = c.TipoEvaluacion ?? string.Empty,
            NumeroEvaluacion   = c.NumeroEvaluacion,
            Puntos             = c.Puntos,
            PuntosMaximos      = c.PuntosMaximos,
            Fecha              = c.Fecha,
            Observaciones      = c.Observaciones
        };
    }
}
