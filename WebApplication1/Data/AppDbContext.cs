using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<Alumno> Alumnos { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<SesionLogin> SesionesLogin { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<TokenRecuperacion> TokensRecuperacion { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Docente> Docentes { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<Grupo> Grupos { get; set; }
        public DbSet<Inscripcion> Inscripciones { get; set; }
        public DbSet<DocenteMateria> DocenteMaterias { get; set; }
        public DbSet<Horario> Horarios { get; set; }
        public DbSet<SesionClase> SesionesClase { get; set; }
        public DbSet<QRGenerado> QRsGenerados { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<Calificacion> Calificaciones { get; set; }
        public DbSet<IntentoLogin> IntentosLogin { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DocenteMateria>()
                .HasKey(dm => new { dm.DocenteID, dm.MateriaID });
        }




    }
}
