namespace WebApplication1.DTOs
{
    /// <summary>
    /// Contrato público de la entidad Materia (PDF §5.1).
    ///
    /// Es la forma que consume el cliente MAUI. El modelo de dominio
    /// <see cref="Models.Materia"/> permanece tal cual para EF Core; este DTO
    /// aplana nombres y añade campos derivados útiles para la capa de UI:
    ///   - Profesor: nombre del primer docente vinculado a la materia (via
    ///     <c>DocenteMateria</c>) o el docente titular del grupo consultado.
    ///   - Horario / Salon: opcionales, útiles cuando la UI solicita la vista
    ///     de "mis materias del día" y necesita estos datos inlineados.
    /// </summary>
    public class MateriaDto
    {
        public int MateriaID { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? Creditos { get; set; }
        public int? HorasSemana { get; set; }
        public bool? Activo { get; set; }

        // Campos derivados (opcionales)
        public string? Profesor { get; set; }
        public string? Horario { get; set; }
        public string? Salon { get; set; }
    }

    /// <summary>Payload de creación/actualización de Materia (sin campos derivados).</summary>
    public record MateriaUpsertDto(
        string Codigo,
        string Nombre,
        string? Descripcion,
        int? Creditos,
        int? HorasSemana,
        bool? Activo);
}
