
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
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;

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

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError(
                ResponseMessages.INVALID_INPUT.FormatMessage("User Id"));

        var user = await _userRepository.GetUserByIdAsync(id, cancellationToken);

        if (user == null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("User"));

        return user;
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
}
