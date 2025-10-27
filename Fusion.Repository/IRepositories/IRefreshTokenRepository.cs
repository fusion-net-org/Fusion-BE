using Fusion.Repository.Data;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task AddTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefreshToken>> GetAllValidTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RemoveExpiredTokensAsync(CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string refreshToken, string? replacedByToken = null, CancellationToken cancellationToken = default);
}
