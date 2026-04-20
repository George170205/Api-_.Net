using System.Linq.Expressions;

namespace WebApplication1.Infrastructure.Repositories
{
    /// <summary>
    /// Contrato genérico de repositorio (PDF §3.2 "Repository Pattern").
    /// Encapsula todas las operaciones CRUD para una entidad y facilita el
    /// testing unitario mediante mocking.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(object id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void Remove(T entity);
        IQueryable<T> Query();
    }
}
