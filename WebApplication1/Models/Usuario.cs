namespace WebApplication1.Models
{
    public class Usuario
    {
        public int UsuarioID { get; set; }

        public int RolID { get; set; }
        public Rol Rol { get; set; }

        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public string Nombre { get; set; }
        public string Apellido { get; set; }

        public string? Telefono { get; set; }

        public DateTime? FechaRegistro { get; set; }
        public DateTime? UltimoAcceso { get; set; }

        public bool Activo { get; set; }

        // Relaciones
        public ICollection<SesionLogin> Sesiones { get; set; }
        public ICollection<Notificacion> Notificaciones { get; set; }
        public ICollection<TokenRecuperacion> Tokens { get; set; }
    }
}
