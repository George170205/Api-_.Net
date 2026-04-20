using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Gestión de Grupos.
    ///
    /// PDF §5.1 "Entidades Principales": Grupo = instancia específica de materia en un semestre.
    /// PDF §2.2 "Perfil Docente": gestión de grupos asignados.
    ///
    /// Rutas:
    ///   GET    /api/grupos
    ///   GET    /api/grupos/{id}
    ///   GET    /api/grupos/docente/{docenteId}
    ///   POST   /api/grupos
    ///   PUT    /api/grupos/{id}
    ///   DELETE /api/grupos/{id}
    /// </summary>
    [ApiController]
    [Route("api/grupos")]
    public class GruposController : ControllerBase
    {
        private readonly AppDbContext _context;
        public GruposController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Grupo>>> Get()
            => await _context.Grupos
                .AsNoTracking()
                .Include(g => g.Materia)
                .ToListAsync();

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Grupo>> GetById(int id)
        {
            var grupo = await _context.Grupos
                .Include(g => g.Materia)
                .Include(g => g.Docente)
                .FirstOrDefaultAsync(g => g.GrupoID == id);
            return grupo == null ? NotFound() : Ok(grupo);
        }

        [HttpGet("docente/{docenteId:int}")]
        public async Task<ActionResult<IEnumerable<Grupo>>> GetByDocente(int docenteId)
            => await _context.Grupos
                .Where(g => g.DocenteID == docenteId)
                .Include(g => g.Materia)
                .AsNoTracking()
                .ToListAsync();

        [HttpPost]
        public async Task<ActionResult<Grupo>> Create(Grupo grupo)
        {
            _context.Grupos.Add(grupo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = grupo.GrupoID }, grupo);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Grupo grupo)
        {
            if (id != grupo.GrupoID) return BadRequest();
            _context.Entry(grupo).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) when (!_context.Grupos.Any(g => g.GrupoID == id))
            { return NotFound(); }
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var grupo = await _context.Grupos.FindAsync(id);
            if (grupo == null) return NotFound();
            _context.Grupos.Remove(grupo);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
