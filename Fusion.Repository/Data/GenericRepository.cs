
using Fusion.Repository.Bases;
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
        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return Task.FromResult(_dbSet.Where(predicate).AsEnumerable());
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public Task<T?> GetByKeyAsync<TKey>(TKey id)
        {
            return _dbSet.FindAsync(id!).AsTask();
        }

        public async Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {

            var query = _dbSet.AsQueryable();

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToListAsync(cancellationToken);

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
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
