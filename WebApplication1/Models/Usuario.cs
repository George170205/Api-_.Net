using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        public int UsuarioID { get; set; }

        public int RolID { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string? Telefono { get; set; }
    }
}
