using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using WebApplication1.Models;
using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController :ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UsuariosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null)
                return BadRequest("Datos inválidos");

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Verificar si ya existe el email
                    string checkQuery = "SELECT COUNT(*) FROM Usuario WHERE Email = @Email";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", request.Email);
                        int count = (int)await checkCmd.ExecuteScalarAsync();

                        if (count > 0)
                            return BadRequest("El correo ya está registrado");
                    }

                    // Insertar usuario
                    string insertQuery = @"
                        INSERT INTO Usuario 
                        (RolID, Email, PasswordHash, Nombre, Apellido, Telefono)
                        VALUES 
                        (@RolID, @Email, @PasswordHash, @Nombre, @Apellido, @Telefono)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@RolID", request.RolID);
                        cmd.Parameters.AddWithValue("@Email", request.Email);
                        cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(request.Password));
                        cmd.Parameters.AddWithValue("@Nombre", request.Nombre);
                        cmd.Parameters.AddWithValue("@Apellido", request.Apellido);
                        cmd.Parameters.AddWithValue("@Telefono",
                            string.IsNullOrEmpty(request.Telefono)
                            ? DBNull.Value
                            : request.Telefono);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return Ok(new { message = "Usuario registrado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error del servidor: {ex.Message}");
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}


