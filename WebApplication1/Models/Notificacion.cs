namespace WebApplication1.Models
{
    public class Notificacion
    {
        public int NotificacionID { get; set; }

        public int UsuarioID { get; set; }
        public Usuario Usuario { get; set; }

        public string Titulo { get; set; }
        public string Mensaje { get; set; }

        public string? Tipo { get; set; }
        public DateTime? FechaEnvio { get; set; }

        public bool? Leido { get; set; }
        public DateTime? FechaLectura { get; set; }
    }
}
