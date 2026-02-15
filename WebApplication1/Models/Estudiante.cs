namespace WebApplication1.Models
{
    public class Estudiante
    {
        public int EstudianteID { get; set; }

        public int UsuarioID { get; set; }
        public Usuario Usuario { get; set; }

        public string Matricula { get; set; }

        public DateTime? FechaNacimiento { get; set; }
        public string? Direccion { get; set; }
        public string? Carrera { get; set; }
        public int? Semestre { get; set; }
    }
}
