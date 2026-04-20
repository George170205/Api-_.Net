using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    ///   POST /api/auth/login    → devuelve access + refresh token.
    ///   POST /api/auth/register → crea un usuario (Estudiante/Docente/Administrador).
    ///   POST /api/auth/refresh  → intercambia refresh token por un nuevo par.
    ///   POST /api/auth/logout   → revoca el refresh token actual.
    ///   GET  /api/auth/me       → datos del usuario autenticado.
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

        public record LoginDto(string Email, string Password);
        public record RegisterDto(string Email, string Password, int RolID, string Nombre, string Apellido, string? Telefono);
        public record RefreshDto(string RefreshToken);

        // =====================================================
        // POST /api/auth/login
        // =====================================================
        [HttpPost("login")]
        [EnableRateLimiting("auth")]
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

            var accessToken = GenerateJwtToken(user);
            var refreshToken = await CreateRefreshTokenAsync(user);
            await _context.SaveChangesAsync();

            // Resolvemos el ID de perfil (Estudiante/Docente) para que el cliente MAUI
            // pueda consumir /api/kardex/estudiante/{id}, /api/grupos/docente/{id}, etc.
            int? estudianteID = await _context.Estudiantes
                .Where(e => e.UsuarioID == user.UsuarioID)
                .Select(e => (int?)e.EstudianteID)
                .FirstOrDefaultAsync();
            int? docenteID = await _context.Docentes
                .Where(d => d.UsuarioID == user.UsuarioID)
                .Select(d => (int?)d.DocenteID)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                token = accessToken,
                refreshToken = refreshToken.Token,
                refreshTokenExpiraEn = refreshToken.FechaExpiracion,
                rol = user.Rol?.NombreRol,
                rolID = user.RolID,
                email = user.Email,
                nombre = $"{user.Nombre} {user.Apellido}".Trim(),
                estudianteID,
                docenteID
            });
        }

        // =====================================================
        // POST /api/auth/register
        // =====================================================
        [HttpPost("register")]
        [EnableRateLimiting("auth")]
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
        // POST /api/auth/refresh
        // =====================================================
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new { message = "Refresh token requerido" });

            var stored = await _context.RefreshTokens
                .Include(r => r.Usuario).ThenInclude(u => u!.Rol)
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken);

            if (stored == null || !stored.Activo)
                return Unauthorized(new { message = "Refresh token inválido o expirado" });
            if (stored.Usuario == null || !stored.Usuario.Activo)
                return Unauthorized(new { message = "Usuario deshabilitado" });

            // Rotación: revoca el anterior y emite uno nuevo.
            stored.FechaRevocacion = DateTime.UtcNow;
            stored.RevocadoPorIP = HttpContext.Connection.RemoteIpAddress?.ToString();

            var nuevoRefresh = await CreateRefreshTokenAsync(stored.Usuario);
            stored.ReemplazadoPorToken = nuevoRefresh.Token;

            var accessToken = GenerateJwtToken(stored.Usuario);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token = accessToken,
                refreshToken = nuevoRefresh.Token,
                refreshTokenExpiraEn = nuevoRefresh.FechaExpiracion
            });
        }

        // =====================================================
        // POST /api/auth/logout
        // =====================================================
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest();

            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken);
            if (stored != null && stored.Activo)
            {
                stored.FechaRevocacion = DateTime.UtcNow;
                stored.RevocadoPorIP = HttpContext.Connection.RemoteIpAddress?.ToString();
                await _context.SaveChangesAsync();
            }
            return NoContent();
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

        private async Task<RefreshToken> CreateRefreshTokenAsync(Usuario user)
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            var tokenStr = Convert.ToBase64String(bytes)
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');

            var refresh = new RefreshToken
            {
                UsuarioID = user.UsuarioID,
                Token = tokenStr,
                FechaEmision = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddDays(30),
                CreadoPorIP = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            await _context.RefreshTokens.AddAsync(refresh);
            return refresh;
        }
    }
}
