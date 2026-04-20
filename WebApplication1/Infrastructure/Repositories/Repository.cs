using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación genérica de <see cref="IRepository{T}"/> sobre EF Core.
    /// PDF §3.2 "Repository Pattern".
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _set;

        public Repository(AppDbContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(object id) => await _set.FindAsync(id);

        public virtual async Task<IEnumerable<T>> GetAllAsync()
            => await _set.AsNoTracking().ToListAsync();

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _set.AsNoTracking().Where(predicate).ToListAsync();

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
            => await _set.FirstOrDefaultAsync(predicate);

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
            => await _set.AnyAsync(predicate);

        public virtual async Task AddAsync(T entity) => await _set.AddAsync(entity);

        public virtual async Task AddRangeAsync(IEnumerable<T> entities) => await _set.AddRangeAsync(entities);

        public virtual void Update(T entity) => _set.Update(entity);

        public virtual void Remove(T entity) => _set.Remove(entity);

        public virtual IQueryable<T> Query() => _set.AsQueryable();
    }
}
