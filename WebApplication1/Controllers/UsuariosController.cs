using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null)
                return BadRequest("Datos inválidos");

            try
            {
                // Verificar si el correo ya existe
                bool existe = await _context.Usuarios
                    .AnyAsync(u => u.Email == request.Email);

                if (existe)
                    return BadRequest("El correo ya está registrado");

                // Crear nuevo usuario
                var usuario = new Usuario
                {
                    RolID = request.RolID,
                    Email = request.Email,
                    PasswordHash = HashPassword(request.Password),
                    Nombre = request.Nombre,
                    Apellido = request.Apellido,
                    Telefono = string.IsNullOrEmpty(request.Telefono)
                        ? null
                        : request.Telefono
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Usuario registrado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error del servidor: {ex.Message}");
            }
        }

        private string HashPassword(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();

            foreach (byte b in bytes)
                builder.Append(b.ToString("x2"));

            return builder.ToString();
        }
    }
}
