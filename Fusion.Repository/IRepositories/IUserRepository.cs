

using Fusion.Repository.Data;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<bool> CheckEmailExistAsync(string email);
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> CheckPassword(string password, byte[] passwordHash, byte[] passwordSalt);
    }
}
