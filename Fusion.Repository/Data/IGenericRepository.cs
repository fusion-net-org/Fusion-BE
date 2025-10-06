
using System.Linq.Expressions;

namespace Fusion.Repository.Data;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> GetAll();
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}
