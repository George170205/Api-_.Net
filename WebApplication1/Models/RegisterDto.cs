using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class RegisterDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public int RolID { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Apellido { get; set; }


        public string? Telefono { get; set; }
    }
}
