
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Fusion.Service.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ICurrentService _currentService;

    public UserService(IUnitOfWork unitOfWork, IUserRepository userRepository, IMapper mapper,
        ICloudinaryService cloudinaryService, ICurrentService currentService)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
        _currentService = currentService;
    }

    public async Task<SelfUserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError(
                ResponseMessages.INVALID_INPUT.FormatMessage("User Id"));

        var user = await _userRepository.GetUserByIdAsync(id, cancellationToken);

        if (user == null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("User"));

        var response = _mapper.Map<SelfUserResponse>(user);
        return response;
    }

    public async Task<PagedResult<CompanyUserResponse>> GetPagedCompanyUsersAsync(CompanyUserPagedRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            throw CustomExceptionFactory.CreateBadRequestError(
                ResponseMessages.INVALID_INPUT);

        var result = await _userRepository.GetPagedCompanyUsersAsync(request, cancellationToken);

        if (result == null || result.Items.Count == 0)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Users"));

        var list = new PagedResult<CompanyUserResponse>
        {
            Items = _mapper.Map<List<CompanyUserResponse>>(result.Items),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
        return list;
    }

    public async Task<PagedResult<AdminUserResponse>> GetPagedAdminUsersAsync(AdminUserPagedRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            throw CustomExceptionFactory.CreateBadRequestError(
                ResponseMessages.INVALID_INPUT);

        var result = await _userRepository.GetPagedAdminUsersAsync(request, cancellationToken);

        if (result == null || result.Items.Count == 0)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Users"));

        var list = new PagedResult<AdminUserResponse>
        {
            Items = _mapper.Map<List<AdminUserResponse>>(result.Items),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
        return list;
    }

    public async Task<SelfUserResponse?> GetSelfUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentService.GetUserId();
        if (userId == Guid.Empty)
            throw CustomExceptionFactory.CreateNotFoundError(
              ResponseMessages.LOGIN_REQUIRED);

        var result = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (result == null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Users"));

        var response = _mapper.Map<SelfUserResponse>(result);

        return response;
    }

    public async Task<SelfUserResponse?> UpdateSelfUserAsync(UpdateSelfUserRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentService.GetUserId();
        if(userId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError(
              ResponseMessages.LOGIN_REQUIRED);

        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("User"));

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Hanlde avatar and map fields from request to user entity
            if (request.Avatar != null)
            {
                if (!string.IsNullOrEmpty(user.Avatar))
                {
                    string publicId = _cloudinaryService.ExtractPublicIdFromUrl(user.Avatar);
                    await _cloudinaryService.DeleteImageAsync(publicId);
                }
                string avatarUrl = await _cloudinaryService.UploadImageAsync(request.Avatar, "avatar-images");
                user.Avatar = avatarUrl;
            }

             _mapper.Map(request, user);

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var result = new SelfUserResponse
            {
                UserName = user.UserName,
                Email = user.Email,
                Phone = user.Phone,
                Avatar = user.Avatar,
                Address = user.Address,
                Gender = user.Gender
            };
               
            return result;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentService.GetUserId();
        if (userId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError(
              ResponseMessages.LOGIN_REQUIRED);

        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("User"));

        using (var hmac = new HMACSHA512(user.PasswordSalt))
        {
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(request.OldPassword));

            if (computedHash.Length != user.PasswordHash.Length ||
                !CryptographicOperations.FixedTimeEquals(computedHash, user.PasswordHash))
            {
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Old password incorrect!"));
            }

            using var newHmac = new HMACSHA512();
            user.PasswordSalt = newHmac.Key;
            user.PasswordHash = newHmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(request.NewPassword));

            user.UpdateAt = DateTime.UtcNow.AddHours(7);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;

        }
    }

    public async Task<PagedResult<SelfUserResponse>> GetAllUsersAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

        var result = await _userRepository.GetAllUsersAsync(request, cancellationToken);

        if (result == null || result.Items.Count == 0)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Users"));

        var list = new PagedResult<SelfUserResponse>
        {
            Items = _mapper.Map<List<SelfUserResponse>>(result.Items),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };

        return list;
    }

    public async Task<Guid?> GetOwnerUserIdByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        if (companyId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError(
                ResponseMessages.INVALID_INPUT.FormatMessage("Company Id"));

        var ownerId = await _userRepository.GetOwnerUserIdByCompanyIdAsync(companyId, cancellationToken);

        if (ownerId == null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Owner User"));

        return ownerId;
    }

}
