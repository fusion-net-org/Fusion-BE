
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
using System.Security.Cryptography;

namespace Fusion.Service.Services;

public class AuthenService : IAuthenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;
    private readonly IMailService _mailService;

    public AuthenService(IUnitOfWork unitOfWork, IUserRepository userRepository,
        IMapper mapper, IJwtService jwtService, IMailService mailService)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _mapper = mapper;
        _jwtService = jwtService;
        _mailService = mailService;
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
                CreateBadRequestError(ResponseMessages.EXISTED.FormatMessage("Email"));
        try
        {

            //3. Map request to user entity
            var user = _mapper.Map<User>(request);

            //4.Create password hash and salt
            using var hmac = new HMACSHA512();
            var passwordSalt = hmac.Key;
            var passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(request.Password));

            user.PasswordSalt = passwordSalt;
            user.PasswordHash = passwordHash;
            user.CreateAt = DateTime.UtcNow.AddHours(7);

            var confirmToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            user.ResetToken = confirmToken;
            user.Status = false;

            await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var confirmLink = $"............/confirm-account?token={confirmToken}";

            var email = new MailRequest
            {
                ToEmail = user.Email,
                Subject = "Confirm your account",
                Body = $"<p>Click the link below to confirm your account:</p><a href='{confirmLink}'>Confirm Account</a>"
            };

            await _mailService.SendEmailAsync(email);
            return true;
        }
        catch
        {
            throw;
        }
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
                CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Email incorrect!"));

        if(user.Status == false)
        {
            throw CustomExceptionFactory.
              CreateBadRequestError(ResponseMessages.CONFIRM_YOUR_ACCOUNT);
        }
        //3. vertify password
        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(request.Password));


        // Optional: check length
        if (computedHash.Length != user.PasswordHash.Length)
            throw CustomExceptionFactory.
                CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Password incorrect!"));

        // So sánh constant-time
        if (!CryptographicOperations.FixedTimeEquals(computedHash, user.PasswordHash))
            throw CustomExceptionFactory.
                CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Password incorrect!"));

        // 4. Generate JWT tokens
        var tokens = await _jwtService.GenerateTokensAsync(user);

        // 5. Map data to LoginResponse
        var response = new LoginResponse
        {
            UserName = user.UserName,
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken
        };
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
        }
        catch (Exception)
        {
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Google token invalid"));
        }

        // Check user existed
        var user = await _userRepository.GetUserByGoogleSubAsync(payload.Subject, cancellationToken);
        if (user != null)
        {
            user.GoogleSub = payload.Subject;
            await _unitOfWork.SaveChangesAsync();
        }
        else
        {
            // if not, create new user
            user = new User
            {
                UserName = payload.Name ?? payload.Email,
                Email = payload.Email,
                GoogleSub = payload.Subject,
                CreateAt = DateTime.UtcNow,
            };

            await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var tokens = await _jwtService.GenerateTokensAsync(user);
        return new LoginResponse
        {
            UserName = user.UserName,
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken
        };
    }

    public async Task<bool> RequestPasswordResetAsync(string email, CancellationToken cancellationToken)
    {
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
        var resetLink = $"........../reset-password?token={token}";

        //send mail
        var mail = new MailRequest
        {
            ToEmail = user.Email,
            Subject = "Password reset request",
            Body = $"<p>Click the link below to reset your password:</p><a href='{resetLink}'>Reset Password</a>"
        };

        await _mailService.SendEmailAsync(mail);

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
        return true;
    }

    public async Task<bool> ConfirmAccountAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(token))
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Token is required"));

        var user = await _userRepository.GetUserByResetTokenAsync(token, cancellationToken);

        if (user == null)
            throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("User or token invalid"));

        // Check if already confirmed
        if (user.Status)
            throw CustomExceptionFactory.CreateBadRequestError("Account already confirmed");
        
        user.Status = true;
        user.ResetToken = null;
        user.UpdateAt = DateTime.UtcNow.AddHours(7);    

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}