
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Users.Responses;

namespace Fusion.Service.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork unitOfWork, IUserRepository userRepository, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _mapper = mapper;
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
    public async Task<PagedResult<UserPageResponse>> GetPagedUsersAsync(UserPagedRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw CustomExceptionFactory.CreateBadRequestError(
                ResponseMessages.INVALID_INPUT);

        var result = await _userRepository.GetPagedUsersAsync(request, cancellationToken);

        if (result == null || result.Items.Count == 0)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Users"));

        var list = new PagedResult<UserPageResponse>
        {
            Items = _mapper.Map<List<UserPageResponse>>(result.Items),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
        return list;
    }
}
