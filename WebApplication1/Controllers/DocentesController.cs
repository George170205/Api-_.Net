using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Catálogo de docentes (PDF §2.3 "Gestión de Docentes"). Expone
    /// información plana + conteo de grupos asignados para alimentar la
    /// pantalla de administración.
    ///
    /// Rutas:
    ///   GET /api/docentes
    ///   GET /api/docentes/{id}
    /// </summary>
    [ApiController]
    [Route("api/docentes")]
    [Authorize]
    public class DocentesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DocentesController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocenteDto>>> Get()
        {
            var docentes = await _context.Docentes
                .AsNoTracking()
                .Include(d => d.Usuario)
                .Select(d => new DocenteDto
                {
                    DocenteID       = d.DocenteID,
                    UsuarioID       = d.UsuarioID,
                    Nombre          = d.Usuario.Nombre,
                    Apellido        = d.Usuario.Apellido,
                    Email           = d.Usuario.Email,
                    NumeroEmpleado  = d.NumeroEmpleado,
                    Departamento    = d.Departamento,
                    TituloAcademico = d.TituloAcademico,
                    Especialidad    = d.Especialidad,
                    GruposAsignados = _context.Grupos.Count(g => g.DocenteID == d.DocenteID)
                })
                .OrderBy(d => d.Apellido)
                .ToListAsync();

            return Ok(docentes);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DocenteDto>> GetById(int id)
        {
            var d = await _context.Docentes
                .AsNoTracking()
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(x => x.DocenteID == id);

            if (d == null) return NotFound();

            var gruposCount = await _context.Grupos.CountAsync(g => g.DocenteID == id);

            return Ok(new DocenteDto
            {
                DocenteID       = d.DocenteID,
                UsuarioID       = d.UsuarioID,
                Nombre          = d.Usuario.Nombre,
                Apellido        = d.Usuario.Apellido,
                Email           = d.Usuario.Email,
                NumeroEmpleado  = d.NumeroEmpleado,
                Departamento    = d.Departamento,
                TituloAcademico = d.TituloAcademico,
                Especialidad    = d.Especialidad,
                GruposAsignados = gruposCount
            });
        }
    }
}
