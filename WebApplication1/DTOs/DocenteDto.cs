namespace WebApplication1.DTOs
{
    /// <summary>
    /// Proyección plana de Docente + Usuario + conteo de grupos (PDF §2.3,
    /// pantalla "Gestión de Docentes" del admin). Evita serializar el grafo
    /// completo Usuario-Docente-Grupos.
    /// </summary>
    public class DocenteDto
    {
        public int DocenteID { get; set; }
        public int UsuarioID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? NumeroEmpleado { get; set; }
        public string? Departamento { get; set; }
        public string? TituloAcademico { get; set; }
        public string? Especialidad { get; set; }
        public int GruposAsignados { get; set; }
    }
}
