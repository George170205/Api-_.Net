using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// CRUD de Materias.
    ///
    /// PDF §5.1 "Entidades Principales": Materia = Asignaturas que se imparten.
    /// PDF §2.3 "Perfil Administrador": gestión de estructura académica.
    ///
    /// Rutas:
    ///   GET    /api/materias
    ///   GET    /api/materias/{id}
    ///   POST   /api/materias
    ///   PUT    /api/materias/{id}
    ///   DELETE /api/materias/{id}
    /// </summary>
    [ApiController]
    [Route("api/materias")]
    public class MateriasController : ControllerBase
    {
        private readonly AppDbContext _context;
        public MateriasController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Materia>>> Get()
            => await _context.Materias.AsNoTracking().ToListAsync();

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Materia>> GetById(int id)
        {
            var materia = await _context.Materias.FindAsync(id);
            return materia == null ? NotFound() : Ok(materia);
        }

        [HttpPost]
        public async Task<ActionResult<Materia>> Create(Materia materia)
        {
            _context.Materias.Add(materia);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = materia.MateriaID }, materia);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Materia materia)
        {
            if (id != materia.MateriaID) return BadRequest();
            _context.Entry(materia).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) when (!_context.Materias.Any(m => m.MateriaID == id))
            { return NotFound(); }
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var materia = await _context.Materias.FindAsync(id);
            if (materia == null) return NotFound();
            _context.Materias.Remove(materia);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
