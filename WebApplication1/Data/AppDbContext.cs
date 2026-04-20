using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    /// <summary>
    /// DbContext principal (PDF §5 "Modelo de Base de Datos").
    ///
    /// Incluye índices estratégicos para los casos de uso de lectura más frecuentes:
    ///   - Login por email  →  índice único en Usuario.Email
    ///   - Validación de QR por token  →  índice único en QRGenerado.TokenUnico
    ///   - Búsqueda de QR activo por sesión  →  índice compuesto (SesionClaseID, Activo)
    ///   - Prevención de asistencia duplicada  →  unique (EstudianteID, SesionClaseID)
    ///   - Dashboard docente por fecha  →  índice en SesionClase.Fecha
    ///
    /// NOTA: el PDF habla de SQL Server 2022, pero el proyecto usa PostgreSQL
    /// (Supabase). La semántica de los índices es idéntica en ambos motores.
    /// </summary>
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
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------- Clave compuesta DocenteMateria --------
            modelBuilder.Entity<DocenteMateria>()
                .HasKey(dm => new { dm.DocenteID, dm.MateriaID });

            // -------- Usuario: email único (login) --------
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Usuario_Email");

            // -------- QRGenerado: token único + búsqueda por sesión --------
            modelBuilder.Entity<QRGenerado>()
                .HasIndex(q => q.TokenUnico)
                .IsUnique()
                .HasDatabaseName("IX_QRGenerado_TokenUnico");

            modelBuilder.Entity<QRGenerado>()
                .HasIndex(q => new { q.SesionClaseID, q.Activo })
                .HasDatabaseName("IX_QRGenerado_Sesion_Activo");

            modelBuilder.Entity<QRGenerado>()
                .HasIndex(q => q.FechaExpiracion)
                .HasDatabaseName("IX_QRGenerado_FechaExpiracion");

            // -------- Asistencia: unique (Estudiante, Sesion) — anti-duplicado --------
            modelBuilder.Entity<Asistencia>()
                .HasIndex(a => new { a.EstudianteID, a.SesionClaseID })
                .IsUnique()
                .HasDatabaseName("IX_Asistencia_Estudiante_Sesion");

            modelBuilder.Entity<Asistencia>()
                .HasIndex(a => a.FechaRegistro)
                .HasDatabaseName("IX_Asistencia_FechaRegistro");

            // -------- Inscripción: lookup por estudiante + grupo --------
            modelBuilder.Entity<Inscripcion>()
                .HasIndex(i => new { i.EstudianteID, i.GrupoID })
                .HasDatabaseName("IX_Inscripcion_Estudiante_Grupo");

            // -------- SesionClase: dashboards por fecha --------
            modelBuilder.Entity<SesionClase>()
                .HasIndex(s => s.Fecha)
                .HasDatabaseName("IX_SesionClase_Fecha");

            modelBuilder.Entity<SesionClase>()
                .HasIndex(s => s.GrupoID)
                .HasDatabaseName("IX_SesionClase_GrupoID");

            // -------- IntentoLogin: rate-limit por email + fecha --------
            modelBuilder.Entity<IntentoLogin>()
                .HasIndex(i => new { i.Email, i.FechaIntento })
                .HasDatabaseName("IX_IntentoLogin_Email_Fecha");

            // -------- RefreshToken: lookup por token + usuario --------
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.Token)
                .IsUnique()
                .HasDatabaseName("IX_RefreshToken_Token");

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.UsuarioID)
                .HasDatabaseName("IX_RefreshToken_UsuarioID");
        }
    }
}
