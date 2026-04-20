namespace WebApplication1.DTOs
{
    /// <summary>Lectura: calificación con datos del alumno y materia ya resueltos.</summary>
    public class CalificacionDto
    {
        public int CalificacionID { get; set; }
        public int InscripcionID { get; set; }
        public int EstudianteID { get; set; }
        public string EstudianteNombre { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public int MateriaID { get; set; }
        public string Materia { get; set; } = string.Empty;
        public string TipoEvaluacion { get; set; } = string.Empty;
        public int? NumeroEvaluacion { get; set; }
        public decimal Puntos { get; set; }
        public decimal PuntosMaximos { get; set; }
        public DateTime? Fecha { get; set; }
        public string? Observaciones { get; set; }
    }

    public record CalificacionUpsertDto(
        int InscripcionID,
        string TipoEvaluacion,
        int? NumeroEvaluacion,
        decimal Puntos,
        decimal PuntosMaximos,
        string? Observaciones);

    /// <summary>
    /// Carga masiva desde la página del docente: una fila por alumno con la
    /// calificación a aplicar para un tipo de evaluación específico.
    /// </summary>
    public record CalificacionBulkItemDto(int InscripcionID, decimal Puntos);
    public record CalificacionBulkDto(
        string TipoEvaluacion,
        decimal PuntosMaximos,
        int? NumeroEvaluacion,
        List<CalificacionBulkItemDto> Items);
}
