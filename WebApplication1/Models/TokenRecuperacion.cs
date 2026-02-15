namespace WebApplication1.Models
{
    public class TokenRecuperacion
    {
        public int TokenRecuperacionID { get; set; }

        public int UsuarioID { get; set; }
        public Usuario Usuario { get; set; }

        public string Token { get; set; }

        public DateTime? FechaGeneracion { get; set; }
        public DateTime FechaExpiracion { get; set; }

        public bool? Utilizado { get; set; }
        public DateTime? FechaUtilizado { get; set; }

        public string? DireccionIP { get; set; }
    }
}
