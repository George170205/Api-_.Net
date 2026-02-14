using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;


[Route("api/[controller]")]
    [ApiController]
    public class AlumnosController : ControllerBase
    {
    private readonly AppBbContext _context;

    public AlumnosController (AppBbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Alumno>>> Get()
    {
        return await _context.Alumnos.ToListAsync();
    }
     [HttpPost]
     public async Task<IActionResult> Post(Alumno alumno)
     {
         _context.Alumnos.Add(alumno);
         await _context.SaveChangesAsync();
         return CreatedAtAction(nameof(Get), new { id = alumno.Id }, alumno);
    }
}
