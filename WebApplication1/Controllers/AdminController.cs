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
    /// Utilidades del perfil Administrador (PDF §2.3).
    ///
    /// Rutas:
    ///   POST /api/admin/import
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Administrador")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _uow;

        public AdminController(AppDbContext context, IUnitOfWork uow)
        {
            _context = context;
            _uow = uow;
        }

        [HttpPost("import")]
        public async Task<ActionResult<ImportResultDto>> Import([FromBody] ImportUsuariosRequest request)
        {
            if (request == null || request.Filas == null || request.Filas.Count == 0)
                return BadRequest(new { message = "Sin filas" });

            var resultado = new ImportResultDto();

            // Emails existentes se descartan para no duplicar (PDF §6).
            var emails = request.Filas.Select(f => f.Email).ToHashSet();
            var existentes = await _context.Usuarios
                .Where(u => emails.Contains(u.Email))
                .Select(u => u.Email)
                .ToListAsync();
            var existentesSet = existentes.ToHashSet(StringComparer.OrdinalIgnoreCase);

            await _uow.BeginTransactionAsync();
            try
            {
                foreach (var fila in request.Filas)
                {
                    if (string.IsNullOrWhiteSpace(fila.Email))
                    {
                        resultado.Omitidos++;
                        resultado.Errores.Add("Fila con email vacío");
                        continue;
                    }
                    if (existentesSet.Contains(fila.Email))
                    {
                        resultado.Omitidos++;
                        continue;
                    }

                    var passwordInicial = string.IsNullOrEmpty(fila.PasswordInicial)
                        ? "Cambiar123!"
                        : fila.PasswordInicial!;

                    var usuario = new Usuario
                    {
                        Email         = fila.Email,
                        PasswordHash  = BCrypt.Net.BCrypt.HashPassword(passwordInicial),
                        RolID         = fila.RolID,
                        Nombre        = fila.Nombre,
                        Apellido      = fila.Apellido,
                        Telefono      = fila.Telefono,
                        Activo        = true,
                        FechaRegistro = DateTime.UtcNow
                    };
                    await _uow.Usuarios.AddAsync(usuario);
                    resultado.Creados++;
                }

                await _uow.CommitTransactionAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
                resultado.Errores.Add(ex.Message);
                return StatusCode(500, resultado);
            }
        }
    }
}
