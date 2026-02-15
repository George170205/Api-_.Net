namespace WebApplication1.Models
{
    public class Grupo
    {
        public int GrupoID { get; set; }

        public int MateriaID { get; set; }
        public Materia Materia { get; set; }

        public int DocenteID { get; set; }
        public Docente Docente { get; set; }

        public string CodigoGrupo { get; set; }
        public string Periodo { get; set; }

        public int? CupoMaximo { get; set; }
        public int? CupoActual { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public bool? Activo { get; set; }
    }
}
