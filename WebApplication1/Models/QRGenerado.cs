namespace WebApplication1.Models
{
    public class QRGenerado
    {
        public int QRGeneradoID { get; set; }

        public int SesionClaseID { get; set; }
        public SesionClase SesionClase { get; set; }

        public string TokenUnico { get; set; }

        public DateTime? FechaGeneracion { get; set; }
        public DateTime FechaExpiracion { get; set; }

        public bool? Activo { get; set; }

        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }

        public int? RadioMetros { get; set; }
    }
}
