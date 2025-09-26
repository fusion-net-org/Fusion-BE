
namespace Fusion.Repository.Data;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> GetAll();
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Romve(T entity);
}
