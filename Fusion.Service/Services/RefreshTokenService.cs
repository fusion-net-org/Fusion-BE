
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Fusion.Service.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RefreshTokenService(IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    /// <summary>
    /// Kiểm tra refresh token có hợp lệ hay không.
    /// </summary>
    public async Task<RefreshToken> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _refreshTokenRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken);

        if (token == null)
            throw CustomExceptionFactory.CreateBadRequestError(
                string.Format(ResponseMessages.INVALID_INPUT));

        if (token.RevokedAt != null)
            throw CustomExceptionFactory.CreateUnauthorizedError("Invalid refresh token.");

        if (token.ExpiresAt <= DateTime.UtcNow)
            throw CustomExceptionFactory.CreateUnauthorizedError("This refresh token has expired.");

        return token;
    }

    /// <summary>
    /// Làm mới token (refresh flow)
    /// </summary>
    public async Task<(string AccessToken, string RefreshToken)> RotateRefreshTokenAsync(
     string oldRefreshToken,
     CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(oldRefreshToken))
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

        // 1️ Validate token cũ
        var oldToken = await ValidateRefreshTokenAsync(oldRefreshToken, cancellationToken);

        // 2️ Lấy thông tin user
        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.FindAsync(x => x.Id == oldToken.UserId, cancellationToken)
            ?? throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("User"));

        // 3️ Thu hồi token cũ
        oldToken.RevokedAt = DateTime.UtcNow;

        // 4️ Tạo refresh token mới
        var refreshTokenLifetimeMinutes = int.Parse(_configuration["JWT:RefreshTokenExpiresInMinutes"] ?? "1440");

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddMinutes(refreshTokenLifetimeMinutes),
            CreatedAt = DateTime.UtcNow,
            ReplacedByToken = oldToken.Token
        };

        oldToken.ReplacedByToken = newRefreshToken.Token;

        _unitOfWork.Repository<RefreshToken>().Update(oldToken);
        await _refreshTokenRepository.AddTokenAsync(newRefreshToken, cancellationToken);

        // 5 Tạo access token mới (⚠️ KHÔNG gọi GenerateTokensAsync — vì hàm đó tạo thêm refresh token)
        var jwtSettings = _configuration.GetSection("JWT");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
    };
      
        if (user.IsSystemAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Role, "User"));
        }
           

        var accessToken = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["AccessTokenExpiresInMinutes"]!)),
            signingCredentials: creds
        );

        string accessTokenStr = new JwtSecurityTokenHandler().WriteToken(accessToken);

        //  Lưu thay đổi
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        //  Trả về cặp token mới (1 access + 1 refresh)
        return (accessTokenStr, newRefreshToken.Token);
    }

    /// <summary>
    /// Thu hồi (revoke) refresh token.
    /// </summary>
    public async Task<bool> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

        var token = await _refreshTokenRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken);

        if (token == null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Refresh token"));

        try
        {
            await _refreshTokenRepository.RevokeTokenAsync(refreshToken, null, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            throw CustomExceptionFactory.CreateInternalServerError(
                $"Failed to revoke refresh token. Detail: {ex.Message}");
        }
    }
    /// <summary>
    /// Dọn dẹp các token đã hết hạn.
    /// </summary>
    public async Task<bool> CleanUpExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _refreshTokenRepository.RemoveExpiredTokensAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            throw CustomExceptionFactory.CreateInternalServerError(
                $"Failed to clean up expired tokens. Detail: {ex.Message}");
        }
    }
}
