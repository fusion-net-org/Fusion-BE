
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Service.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Fusion.Service.Services;

public class JwtService : IJwtService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public JwtService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }
    public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user)
    {
       if(user == null)
            throw CustomExceptionFactory.CreateBadRequestError("User cannot be null");

        // --- 1. Generate Access Token ---
        var jwtSettings = _configuration.GetSection("JWT");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        };
        claims.Add(new Claim(ClaimTypes.Role, "User"));
        if (user.IsSystemAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));


        var accessToken = new JwtSecurityToken(
          issuer: jwtSettings["Issuer"],
          audience: jwtSettings["Audience"],
          claims: claims,
          expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["AccessTokenExpiresInMinutes"]!)),
          signingCredentials: creds
         );

        string accessTokenStr = new JwtSecurityTokenHandler().WriteToken(accessToken);

        // --- 2. Generate Refresh Token ---
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(jwtSettings["RefreshTokenExpiresInMinutes"]!)),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return (accessTokenStr, refreshToken.Token);
    }
}
