using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.UserLog;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.UserLog.Responses;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Service.Services;

public class UserLogService : IUserLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserLogRepository _userLogRepository;
    private readonly IMapper _mapper;

    public UserLogService(IUnitOfWork unitOfWork, IUserLogRepository userLogRepository, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _userLogRepository = userLogRepository;
        _mapper = mapper;
    }

    public async Task<UserLog> CreateLog(UserLog log, CancellationToken cancellationToken = default)
    {
        if (log == null)
            throw CustomExceptionFactory.CreateBadRequestError(
                ResponseMessages.INVALID_INPUT.FormatMessage("log"));

        try
        {
            log.CreatedAt = DateTime.UtcNow;
            log.IsDeleted = false;
            await _unitOfWork.Repository<UserLog>().AddAsync(log);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return log;
        }
        catch (DbUpdateException dbEx)
        {
            throw new Exception("Database update failed when creating log.", dbEx);
        }
    }

    // 1) Lấy tất cả user logs (filter + sort + paging)
    public Task<PagedResult<UserLog>> GetAllUserLogAsync(
        UserLogSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        request ??= new UserLogSearchRequest();
        if (string.IsNullOrWhiteSpace(request.SortColumn))
        {
            request.SortColumn = nameof(UserLog.CreatedAt);
            request.SortDescending = true;
        }

        return _userLogRepository.GetAllUserLogAsync(request, cancellationToken);
    }

    // 2) Lấy user logs theo ActorUserId (filter + sort + paging)
    public Task<PagedResult<UserLog>> GetUserLogByIdAsync(
        Guid actorUserId,
        UserLogSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        request ??= new UserLogSearchRequest();
        if (string.IsNullOrWhiteSpace(request.SortColumn))
        {
            request.SortColumn = nameof(UserLog.CreatedAt);
            request.SortDescending = true;
        }

        return _userLogRepository.GetUserLogByIdAsync(actorUserId, request, cancellationToken);
    }

    public async Task<PagedResult<UserLogResponse>> GetUserLogByUserIdAsync(
        Guid actorUserId,
        UserLogSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        request ??= new UserLogSearchRequest();
        if (string.IsNullOrWhiteSpace(request.SortColumn))
        {
            request.SortColumn = nameof(UserLog.CreatedAt);
            request.SortDescending = true;
        }

        var userLog = await _userLogRepository.GetUserLogByUserIdAsync(actorUserId, request, cancellationToken);


        return new PagedResult<UserLogResponse>
        {
            Items = _mapper.Map<List<UserLogResponse>>(userLog.Items),
            TotalCount = userLog.TotalCount,
            PageNumber = userLog.PageNumber,
            PageSize = userLog.PageSize
        };

    }
}
