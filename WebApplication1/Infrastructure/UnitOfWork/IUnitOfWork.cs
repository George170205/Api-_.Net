using WebApplication1.Infrastructure.Repositories;
using WebApplication1.Models;

namespace WebApplication1.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Unit of Work — PDF §3.2.
    /// Gestiona transacciones que involucran múltiples repositorios, con commits
    /// atómicos y rollback automático en caso de error.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Usuario> Usuarios { get; }
        IRepository<Estudiante> Estudiantes { get; }
        IRepository<Docente> Docentes { get; }
        IRepository<Materia> Materias { get; }
        IRepository<Grupo> Grupos { get; }
        IRepository<Inscripcion> Inscripciones { get; }
        IRepository<SesionClase> SesionesClase { get; }
        IRepository<QRGenerado> QRsGenerados { get; }
        IRepository<Asistencia> Asistencias { get; }
        IRepository<Calificacion> Calificaciones { get; }
        IRepository<Notificacion> Notificaciones { get; }

        /// <summary>Guarda todos los cambios pendientes de forma atómica.</summary>
        Task<int> SaveChangesAsync();

        /// <summary>Inicia una transacción explícita (opcional).</summary>
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
