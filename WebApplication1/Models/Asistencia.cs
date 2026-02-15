namespace WebApplication1.Models
{
    public class Asistencia
    {
        public int AsistenciaID { get; set; }

        public int SesionClaseID { get; set; }
        public SesionClase SesionClase { get; set; }

        public int EstudianteID { get; set; }
        public Estudiante Estudiante { get; set; }

        public int? QRGeneradoID { get; set; }
        public QRGenerado QRGenerado { get; set; }

        public DateTime? FechaRegistro { get; set; }

        public string? Estado { get; set; }

        public int? MinutosTarde { get; set; }

        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }

        public string? Observaciones { get; set; }
    }
}
