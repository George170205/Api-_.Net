namespace WebApplication1.Models
{
    public class DocenteMateria
    {
        public int DocenteID { get; set; }
        public Docente Docente { get; set; }

        public int MateriaID { get; set; }
        public Materia Materia { get; set; }

        public DateTime? FechaAsignacion { get; set; }
        public bool? Activo { get; set; }
    }
}
