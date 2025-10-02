
using Fusion.Repository.IRepositories;

namespace Fusion.Repository.Data;

public interface IUnitOfWork
{
    //Khai báo để gọi chung
    ICompanyRepository companyRepository { get; }

    IUserRepository userRepository { get; }

    IGenericRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
