using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// CRUD de Materias.
    ///
    /// PDF §5.1 "Entidades Principales": Materia = Asignaturas que se imparten.
    /// PDF §2.3 "Perfil Administrador": gestión de estructura académica.
    ///
    /// El response devuelve <see cref="MateriaDto"/> con campos planos para
    /// alinearse con el cliente MAUI; las operaciones de escritura aceptan
    /// <see cref="MateriaUpsertDto"/>.
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
        public async Task<ActionResult<IEnumerable<MateriaDto>>> Get()
        {
            var materias = await _context.Materias.AsNoTracking().ToListAsync();

            // Para cada materia, buscamos un docente "principal" (el primero
            // vinculado vía DocenteMateria). Si no hay, Profesor queda null.
            var profesoresPorMateria = await _context.DocenteMaterias
                .AsNoTracking()
                .Include(dm => dm.Docente).ThenInclude(d => d.Usuario)
                .GroupBy(dm => dm.MateriaID)
                .Select(g => new
                {
                    MateriaID = g.Key,
                    Docente = g.Select(x => x.Docente.Usuario).FirstOrDefault()
                })
                .ToListAsync();

            var profMap = profesoresPorMateria.ToDictionary(x => x.MateriaID, x => x.Docente);

            return materias.Select(m => ToDto(m, profMap.GetValueOrDefault(m.MateriaID))).ToList();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MateriaDto>> GetById(int id)
        {
            var materia = await _context.Materias.FindAsync(id);
            if (materia == null) return NotFound();

            var docente = await _context.DocenteMaterias
                .AsNoTracking()
                .Include(dm => dm.Docente).ThenInclude(d => d.Usuario)
                .Where(dm => dm.MateriaID == id)
                .Select(dm => dm.Docente.Usuario)
                .FirstOrDefaultAsync();

            return ToDto(materia, docente);
        }

        [HttpPost]
        public async Task<ActionResult<MateriaDto>> Create([FromBody] MateriaUpsertDto dto)
        {
            var materia = new Materia
            {
                CodigoMateria = dto.Codigo,
                NombreMateria = dto.Nombre,
                Descripcion   = dto.Descripcion,
                Creditos      = dto.Creditos,
                HorasSemana   = dto.HorasSemana,
                Activo        = dto.Activo ?? true
            };
            _context.Materias.Add(materia);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = materia.MateriaID }, ToDto(materia, null));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] MateriaUpsertDto dto)
        {
            var materia = await _context.Materias.FindAsync(id);
            if (materia == null) return NotFound();

            materia.CodigoMateria = dto.Codigo;
            materia.NombreMateria = dto.Nombre;
            materia.Descripcion   = dto.Descripcion;
            materia.Creditos      = dto.Creditos;
            materia.HorasSemana   = dto.HorasSemana;
            materia.Activo        = dto.Activo ?? materia.Activo;

            await _context.SaveChangesAsync();
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

        // ---------------------------------------------------------------
        private static MateriaDto ToDto(Materia m, Usuario? docente) => new()
        {
            MateriaID   = m.MateriaID,
            Codigo      = m.CodigoMateria ?? string.Empty,
            Nombre      = m.NombreMateria ?? string.Empty,
            Descripcion = m.Descripcion,
            Creditos    = m.Creditos,
            HorasSemana = m.HorasSemana,
            Activo      = m.Activo,
            Profesor    = docente == null ? null : $"{docente.Nombre} {docente.Apellido}".Trim(),
            Horario     = null,
            Salon       = null
        };
    }
}
