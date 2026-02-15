namespace WebApplication1.Models
{
    public class SesionClase
    {
        public int SesionClaseID { get; set; }

        public int GrupoID { get; set; }
        public Grupo Grupo { get; set; }

        public DateTime Fecha { get; set; }

        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }

        public string? Aula { get; set; }
        public string? Tema { get; set; }
        public string? Estado { get; set; }
        public string? Observaciones { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public ICollection<QRGenerado> QRs { get; set; }
        public ICollection<Asistencia> Asistencias { get; set; }
    }
}
