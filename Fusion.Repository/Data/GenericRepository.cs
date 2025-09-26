using Fusion.Repository.Bases.Page;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Fusion.Repository.Data
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly FusionDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(FusionDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return Task.FromResult(_dbSet.Where(predicate).AsEnumerable());
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet.AsQueryable();
        }

        public Task<T?> GetByKeyAsync<TKey>(TKey id)
        {
            return _dbSet.FindAsync(id!).AsTask();
        }

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }
        public void Romve(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
    }
}
