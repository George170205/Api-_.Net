using System.Text.RegularExpressions;

namespace WebApplication1.Models
{
    public class Materia
    {
        public int MateriaID { get; set; }

        public string CodigoMateria { get; set; }
        public string NombreMateria { get; set; }

        public string? Descripcion { get; set; }
        public int? Creditos { get; set; }
        public int? HorasSemana { get; set; }

        public bool? Activo { get; set; }

        public ICollection<Grupo> Grupos { get; set; }
    }
}
