using Microsoft.EntityFrameworkCore.Storage;
using WebApplication1.Data;
using WebApplication1.Infrastructure.Repositories;
using WebApplication1.Models;

namespace WebApplication1.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Implementación de <see cref="IUnitOfWork"/> sobre EF Core.
    /// PDF §3.2 — "Unit of Work": centraliza el commit transaccional de múltiples
    /// repositorios. Repositorios se crean de forma perezosa (lazy) para minimizar
    /// asignaciones cuando una request sólo toca un subconjunto de entidades.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _currentTransaction;

        // Backing fields para lazy init.
        private IRepository<Usuario>? _usuarios;
        private IRepository<Estudiante>? _estudiantes;
        private IRepository<Docente>? _docentes;
        private IRepository<Materia>? _materias;
        private IRepository<Grupo>? _grupos;
        private IRepository<Inscripcion>? _inscripciones;
        private IRepository<SesionClase>? _sesionesClase;
        private IRepository<QRGenerado>? _qrsGenerados;
        private IRepository<Asistencia>? _asistencias;
        private IRepository<Calificacion>? _calificaciones;
        private IRepository<Notificacion>? _notificaciones;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IRepository<Usuario> Usuarios        => _usuarios        ??= new Repository<Usuario>(_context);
        public IRepository<Estudiante> Estudiantes  => _estudiantes     ??= new Repository<Estudiante>(_context);
        public IRepository<Docente> Docentes        => _docentes        ??= new Repository<Docente>(_context);
        public IRepository<Materia> Materias        => _materias        ??= new Repository<Materia>(_context);
        public IRepository<Grupo> Grupos            => _grupos          ??= new Repository<Grupo>(_context);
        public IRepository<Inscripcion> Inscripciones => _inscripciones ??= new Repository<Inscripcion>(_context);
        public IRepository<SesionClase> SesionesClase => _sesionesClase ??= new Repository<SesionClase>(_context);
        public IRepository<QRGenerado> QRsGenerados => _qrsGenerados    ??= new Repository<QRGenerado>(_context);
        public IRepository<Asistencia> Asistencias  => _asistencias     ??= new Repository<Asistencia>(_context);
        public IRepository<Calificacion> Calificaciones => _calificaciones ??= new Repository<Calificacion>(_context);
        public IRepository<Notificacion> Notificaciones => _notificaciones ??= new Repository<Notificacion>(_context);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null) return;
            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction == null) return;
            try
            {
                await _context.SaveChangesAsync();
                await _currentTransaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction == null) return;
            try
            {
                await _currentTransaction.RollbackAsync();
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
