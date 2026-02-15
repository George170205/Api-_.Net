namespace WebApplication1.Models
{
    public class Rol
    {
        public int RolID { get; set; }
        public string NombreRol { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public bool Activo { get; set; }

        public ICollection<Usuario> Usuarios { get; set; }
    }
}
