namespace WebApplication1.Models
{
    public class IntentoLogin
    {
        public int IntentoLoginID { get; set; }

        public string Email { get; set; }

        public int? UsuarioID { get; set; }
        public Usuario Usuario { get; set; }

        public DateTime? FechaIntento { get; set; }

        public bool Exitoso { get; set; }

        public string? MotivoFallo { get; set; }

        public string? DireccionIP { get; set; }
        public string? UserAgent { get; set; }

        public string? Pais { get; set; }
        public string? Ciudad { get; set; }
    }
}
