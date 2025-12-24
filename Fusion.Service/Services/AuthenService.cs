
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Email;
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Security.Cryptography;

namespace Fusion.Service.Services;

public class AuthenService : IAuthenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;
    private readonly IMailService _mailService;
    private readonly IUserLogService _userLogService;
    private readonly IConfiguration _config;
    private readonly IUserSubscriptionService _userSub;

    public AuthenService(IUnitOfWork unitOfWork, IUserRepository userRepository,
        IMapper mapper, IJwtService jwtService, IMailService mailService, IUserLogService userLogService, IConfiguration configuration, IUserSubscriptionService userSub)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _mapper = mapper;
        _jwtService = jwtService;
        _mailService = mailService;
        _userLogService = userLogService;
        _config = configuration;
        _userSub = userSub;
    }
    public async Task<bool> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        //1.check request not null
        if (request == null)
            throw CustomExceptionFactory.
                CreateBadRequestError(ResponseMessages.INVALID_INPUT);

        // 2. check email exist
        if (await _userRepository.CheckEmailExistAsync(request.Email, cancellationToken))
            throw CustomExceptionFactory.
                CreateBadRequestError(ResponseMessages.EXISTED, "Email");

        //3. Map request to user entity
        var user = _mapper.Map<User>(request);

        //4.Create password hash and salt
        using var hmac = new HMACSHA512();
        var passwordSalt = hmac.Key;
        var passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(request.Password));

        user.Status = false;
        user.PasswordSalt = passwordSalt;
        user.PasswordHash = passwordHash;
        user.CreateAt = DateTime.UtcNow.AddHours(7);

        await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await SendVerificationEmailAsync(user.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            throw;
        }
        return true;
    }
    public async Task<bool> SendVerificationEmailAsync(Guid userId, CancellationToken ct = default)
    {
        // Get user (tracking to update)
        var user = await _userRepository.GetUserByIdAsync(userId, ct);
        if (user == null)
            throw CustomExceptionFactory.CreateNotFoundError("Not found user.");

        var rawToken = GenerateUrlSafeToken(32);
        user.ResetToken = rawToken;

        await _unitOfWork.SaveChangesAsync(ct);

        // Build verify URL cho FE: FE sẽ đọc ?vtoken=... rồi gọi API /auth/verify
        var origin = "http://localhost:5173/login";

        var verifyUrl = $"{origin}/?vtoken={Uri.EscapeDataString(rawToken)}";

        var displayName = WebUtility.HtmlEncode(user.UserName ?? user.Email ?? "there");
        var subject = "[FUSION] Confirm Account";
        var html = $@"
<!doctype html>
<html>
  <body style='font-family:Inter,Arial,sans-serif;background:#f7f9fb;padding:24px;color:#0f172a'>
    <div style='max-width:560px;margin:auto;background:#fff;border-radius:12px;padding:24px;box-shadow:0 10px 30px rgba(46,139,255,0.15)'>
      <h2 style='margin:0 0 12px;color:#1e293b'>Confirm your account</h2>
      <p>Hello {displayName},</p>
      <p>Click the button below to verify your email and activate your FUSION account:</p>
      <p style='margin:20px 0'>
        <a href='{verifyUrl}' style='background:#2E8BFF;color:white;padding:12px 18px;border-radius:10px;text-decoration:none;display:inline-block'>
          Verify now
        </a>
      </p>
      <p>If the button doesn't work, paste the link into your browser: <br/><a href='{verifyUrl}'>{verifyUrl}</a></p>
      <p style='margin-top:24px;color:#475569;font-size:12px'>Link will expire after <b>24 hours</b>.</p>
    </div>
  </body>
</html>";

        try
        {
            // Nếu IMailService dùng MailRequest:
            var mail = new MailRequest
            {
                ToEmail = user.Email ?? "",
                Subject = subject,
                Body = html,
                Attachments = null
            };
            // await _mailService.SendEmailAsync(mail);  

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    public async Task<bool> EmailVerificationAsync(string token, CancellationToken cancellationToken = default)
    {
        var result = await _userRepository.EmailVerificationAsync(token, cancellationToken);
        return result;
    }
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        // 1. validate input
        if (request == null)
            throw CustomExceptionFactory.
                CreateBadRequestError(ResponseMessages.INVALID_INPUT);

        // 2. check email exist
        var user = await _userRepository.GetUserByEmailAsync(request.Email, cancellationToken);
        if (user == null)
            throw CustomExceptionFactory.
                CreateBadRequestError(ResponseMessages.INVALID_INPUT, "Email incorrect!");
        if (user.Status == false)
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.BAD_REQUEST, "Please verify gmail");

        //3. vertify password
        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(request.Password));


        // Optional: check length
        if (computedHash.Length != user.PasswordHash.Length)
            throw CustomExceptionFactory.
                CreateBadRequestError(ResponseMessages.BAD_REQUEST, "Password incorrect!");

        // So sánh constant-time
        if (!CryptographicOperations.FixedTimeEquals(computedHash, user.PasswordHash))
            throw CustomExceptionFactory.
                CreateBadRequestError(ResponseMessages.BAD_REQUEST, "Password incorrect!");

        // 4. Generate JWT tokens
        var tokens = await _jwtService.GenerateTokensAsync(user);

        // 5. Map data to LoginResponse
        var response = new LoginResponse
        {
            UserName = user.UserName,
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken
        };
        var userLog = new UserLog
        {
            ActorUserId = user.Id,
            Title = "Login",
            Description = $"User {user.UserName} logged into the system."
        };
        await _userLogService.CreateLog(userLog);
        await _userSub.EnsureAutoMonthlyForUserAsync(user.Id);
        return response;
    }
    public async Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

        //Validate the token
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);


            // Check user existed
            var user = await _userRepository.GetUserByGoogleSubAsync(payload.Subject, cancellationToken);
            if (user != null)
            {
                user.GoogleSub = payload.Subject;
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                using var rng = RandomNumberGenerator.Create();
                var salt = new byte[128]; rng.GetBytes(salt);
                var hash = new byte[64]; rng.GetBytes(hash);
                // if not, create new user
                user = new User
                {
                    UserName = payload.Name ?? payload.Email,
                    Email = payload.Email,
                    GoogleSub = payload.Subject,
                    CreateAt = DateTime.UtcNow,
                    Status = true,
                    PasswordHash = hash,
                    PasswordSalt = salt
                };

                await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                var userLog = new UserLog
                {
                    ActorUserId = user.Id,
                    Title = "Login-Google",
                    Description = $"User {user.UserName} logged into the system"
                };
                await _userLogService.CreateLog(userLog);
            }

            var tokens = await _jwtService.GenerateTokensAsync(user);
            return new LoginResponse
            {
                UserName = user.UserName,
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken
            };
        }
        catch (Exception ex)
        {
            throw;
            //throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Google token invalid"));
        }
    }
    public async Task<bool> RequestPasswordResetAsync(string email, string device, CancellationToken cancellationToken)
    {
        var resetLink = "";

        // 1. Get user by email
        var user = await _userRepository.GetUserByEmailAsync(email, cancellationToken);
        if (user == null)
            throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Email"));

        // Generic token and save
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        user.ResetToken = token;
        user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // create link reset password page
        if (device.ToLower().Equals("mobile"))
        {
            resetLink = $"https://www.fusion.info.vn/reset-password?token={token}";
        }
        else
        {
            resetLink = $"https://www.fusion.info.vn/reset-password?token={token}";
        }

        //send mail
        var mail = new MailRequest
        {
            ToEmail = user.Email,
            Subject = "Password reset request",
            Body = $"<p>Click the link below to reset your password:</p><a href='{resetLink}'>Reset Password</a>"
        };

        await _mailService.SendEmailAsync(mail);
        var userLog = new UserLog
        {
            ActorUserId = user.Id,
            Title = "Request password",
            Description = $"User {user.UserName} has requested to reset password."
        };
        await _userLogService.CreateLog(userLog);
        return true;
    }
    public async Task<bool> ResetPasswordAsync(string resetToken, string newPassword, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByResetTokenAsync(resetToken, cancellationToken);
        if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
            throw CustomExceptionFactory.CreateBadRequestError("Invalid or expired token");

        using var hmac = new HMACSHA512();
        user.PasswordSalt = hmac.Key;
        user.PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(newPassword));

        user.ResetToken = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var userLog = new UserLog
        {
            ActorUserId = user.Id,
            Title = "Reset password",
            Description = $"User {user.UserName} changed password."
        };
        await _userLogService.CreateLog(userLog);
        return true;
    }
    private static string GenerateUrlSafeToken(int bytes = 32)
    {
        var data = new byte[bytes];
        System.Security.Cryptography.RandomNumberGenerator.Fill(data);
        return Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

}