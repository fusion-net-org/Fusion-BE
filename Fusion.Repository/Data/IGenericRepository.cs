

using Fusion.Repository.Bases;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Fusion.Repository.Data;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity);
    void Update(T entity);
    void Romve(T entity);
    Task<T?> GetByKeyAsync<TKey>(TKey id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
}
