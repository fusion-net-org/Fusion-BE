

using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    private readonly FusionDbContext _context;
    public RefreshTokenRepository(FusionDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy refresh token theo chuỗi token.
    /// </summary>
    public async Task<RefreshToken?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
       => await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);

    /// <summary>
    /// Thêm mới refresh token vào DB.
    /// </summary>
    public async Task AddTokenAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(token, cancellationToken);
    }

    /// <summary>
    /// Lấy tất cả token hợp lệ của user (chưa hết hạn và chưa bị thu hồi).
    /// </summary>
    public async Task<IEnumerable<RefreshToken>> GetAllValidTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
           .Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow)
           .AsNoTracking()
           .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Thu hồi (revoke) token cụ thể.
    /// </summary>
    public async Task RevokeTokenAsync(string refreshToken, string? replacedByToken = null, CancellationToken cancellationToken = default)
    {
        var token = await GetByRefreshTokenAsync(refreshToken, cancellationToken);
        if (token == null) return;

        token.RevokedAt = DateTime.UtcNow;
        token.ReplacedByToken = replacedByToken;

        _context.RefreshTokens.Update(token);
        await _context.SaveChangesAsync(cancellationToken);
    }


    /// <summary>
    /// Xóa tất cả refresh token đã hết hạn.
    /// </summary>
    public async Task RemoveExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(x => x.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (expiredTokens.Count > 0)
        {
            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
