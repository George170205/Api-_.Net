namespace WebApplication1.Models
{
    public class Inscripcion
    {
        public int InscripcionID { get; set; }

        public int GrupoID { get; set; }
        public Grupo Grupo { get; set; }

        public int EstudianteID { get; set; }
        public Estudiante Estudiante { get; set; }

        public DateTime? FechaInscripcion { get; set; }

        public string? Estado { get; set; }

        public decimal? CalificacionFinal { get; set; }
    }
}
