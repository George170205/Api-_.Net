namespace WebApplication1.Models
{
    public class Calificacion
    {
        public int CalificacionID { get; set; }

        public int InscripcionID { get; set; }
        public Inscripcion Inscripcion { get; set; }

        public string TipoEvaluacion { get; set; }

        public int? NumeroEvaluacion { get; set; }

        public decimal Puntos { get; set; }
        public decimal PuntosMaximos { get; set; }

        public decimal? Porcentaje { get; set; }

        public DateTime? Fecha { get; set; }

        public string? Observaciones { get; set; }
    }
}
