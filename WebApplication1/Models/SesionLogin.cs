namespace WebApplication1.Models
{
    public class SesionLogin
    {
        public int SesionLoginID { get; set; }

        public int UsuarioID { get; set; }
        public Usuario Usuario { get; set; }

        public string TokenSesion { get; set; }

        public DateTime? FechaInicio { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public DateTime? FechaCierre { get; set; }

        public string? DireccionIP { get; set; }
        public string? UserAgent { get; set; }
        public string? Dispositivo { get; set; }

        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }

        public string? Estado { get; set; }
        public int? IntentosLogin { get; set; }
        public DateTime? UltimaActividad { get; set; }
    }
}
