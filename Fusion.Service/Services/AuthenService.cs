
using AutoMapper;
using Azure;
using Azure.Core;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
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

    public AuthenService(IUnitOfWork unitOfWork, IUserRepository userRepository,
        IMapper mapper, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _mapper = mapper;
        _jwtService = jwtService;
    }
    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        //1.check request not null
        if (request == null)
            throw CustomExceptionFactory.
                CreateBadRequestError(ResponseMessages.INVALID_INPUT);

        // 2. check email exist
        if (_userRepository.CheckEmailExistAsync(request.Email).Result)
            throw CustomExceptionFactory.
                CreateBadRequestError(ResponseMessages.EXISTED.FormatMessage("Email"));

        //3. Map request to user entity
        var user = _mapper.Map<User>(request);

        //4.Create password hash and salt
        using var hmac = new HMACSHA512();
        var passwordSalt = hmac.Key;
        var passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(request.Password));

        user.PasswordSalt = passwordSalt;
        user.PasswordHash = passwordHash;

        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // 1. validate input
        if (request == null)
            throw CustomExceptionFactory.
                CreateBadRequestError(ResponseMessages.INVALID_INPUT);

        // 2. check email exist
        var user = await _userRepository.GetUserByEmailAsync(request.Email);
        if (user == null)
            throw CustomExceptionFactory.
                CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Email incorrect!"));

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

    public async Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request)
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
        var user = await _userRepository.GetUserByGoogleSubAsync(payload.Subject);
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

            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        var tokens = await _jwtService.GenerateTokensAsync(user);
        return new LoginResponse
        {
            UserName = user.UserName,
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken
        };
    }
}
