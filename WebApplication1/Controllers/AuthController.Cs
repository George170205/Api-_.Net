using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Controlador de Autenticación.
    ///
    /// Referencias al PDF "Propuesta_Sistema_Asistencias_QR":
    ///   §3.2 "Patrón de Diseño"  → JWT Authentication con claims por rol.
    ///   §6   "Seguridad del Sistema".
    ///
    /// Rutas (alineadas con <see cref="MauiApp1.Services.ApiService"/>):
    ///   POST /api/auth/login    → devuelve JWT + datos de usuario.
    ///   POST /api/auth/register → crea un usuario (Estudiante/Docente/Administrador).
    ///   GET  /api/auth/me       → datos del usuario autenticado (requiere token).
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // =====================================================
        // POST /api/auth/login
        // =====================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email y contraseña son obligatorios" });

            var user = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return Unauthorized(new { message = "Credenciales incorrectas" });

            if (!VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Credenciales incorrectas" });

            if (!user.Activo)
                return Unauthorized(new { message = "Cuenta deshabilitada" });

            user.UltimoAcceso = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            // Forma esperada por LoginResponse (MauiApp1.src.Core.Models.LoginResponse)
            return Ok(new
            {
                token,
                rol = user.Rol?.NombreRol,
                rolID = user.RolID,
                email = user.Email,
                nombre = $"{user.Nombre} {user.Apellido}".Trim()
            });
        }

        // =====================================================
        // POST /api/auth/register
        // =====================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            if (request == null)
                return BadRequest(new { message = "Datos inválidos" });

            if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { message = "El correo ya está registrado" });

            var usuario = new Usuario
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RolID = request.RolID,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Telefono = string.IsNullOrEmpty(request.Telefono) ? null : request.Telefono,
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario registrado correctamente", usuarioID = usuario.UsuarioID });
        }

        // =====================================================
        // GET /api/auth/me
        // =====================================================
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var email = User?.FindFirstValue(ClaimTypes.Email)
                        ?? User?.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null) return NotFound();

            return Ok(new
            {
                usuario.UsuarioID,
                usuario.Email,
                usuario.Nombre,
                usuario.Apellido,
                rol = usuario.Rol?.NombreRol,
                usuario.RolID
            });
        }

        // =====================================================
        // Helpers
        // =====================================================
        private static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;

            // BCrypt hashes siempre comienzan con "$2" → $2a$, $2b$, $2y$…
            if (storedHash.StartsWith("$2"))
            {
                try { return BCrypt.Net.BCrypt.Verify(password, storedHash); }
                catch { return false; }
            }

            // Compatibilidad hacia atrás: usuarios creados por UsuariosController con SHA-256.
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            var sb = new StringBuilder();
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return storedHash.Equals(sb.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private string GenerateJwtToken(Usuario user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims personalizados por rol (PDF §3.2 "JWT Authentication")
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UsuarioID.ToString()),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new("RolID", user.RolID.ToString()),
                new(ClaimTypes.Role, user.Rol?.NombreRol ?? string.Empty),
                new("Nombre", user.Nombre ?? string.Empty)
            };

            var expireMinutes = double.TryParse(jwtSettings["ExpireMinutes"], out var m) ? m : 60;

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
