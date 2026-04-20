namespace WebApplication1.DTOs
{
    /// <summary>
    /// Cada fila del CSV que el admin carga (PDF §2.3).
    /// Rol: 1=Alumno, 2=Docente, 3=Administrador (alineado con tabla Rol).
    /// </summary>
    public record ImportUsuarioRow(
        string Email,
        string Nombre,
        string Apellido,
        int RolID,
        string? Telefono,
        string? PasswordInicial);

    public record ImportUsuariosRequest(List<ImportUsuarioRow> Filas);

    public class ImportResultDto
    {
        public int Creados { get; set; }
        public int Omitidos { get; set; }
        public List<string> Errores { get; set; } = new();
    }
}
