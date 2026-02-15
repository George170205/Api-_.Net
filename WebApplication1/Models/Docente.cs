namespace WebApplication1.Models
{
    public class Docente
    {
        public int DocenteID { get; set; }

        public int UsuarioID { get; set; }
        public Usuario Usuario { get; set; }

        public string NumeroEmpleado { get; set; }

        public string? Departamento { get; set; }
        public string? TituloAcademico { get; set; }
        public string? Especialidad { get; set; }
        public DateTime? FechaContratacion { get; set; }
    }
}
