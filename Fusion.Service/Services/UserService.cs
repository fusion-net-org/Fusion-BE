
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectBoard;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Bases.Page.UserLog;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Repository.ViewModels.Users;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Task.Response;
using Fusion.Service.ViewModels.UserLog.Responses;
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;
using System.Security.Cryptography;

namespace Fusion.Service.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ICurrentService _currentService;
    private readonly IUserLogService _userLog;

    public UserService(IUnitOfWork unitOfWork, IUserRepository userRepository, IMapper mapper,
        ICloudinaryService cloudinaryService, ICurrentService currentService, IUserLogService userLog, ITaskRepository taskRepository)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _taskRepository = taskRepository;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
        _currentService = currentService;
        _userLog = userLog;
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
    public async Task<List<RoleDto>> GetRolesByUserAndCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError("User Id is invalid");

        if (companyId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError("Company Id is invalid");

        var roles = await _userRepository.GetRolesByUserAndCompanyAsync(userId, companyId, cancellationToken);

        return roles;
    }
    public async Task<User?> GetFullInfoByIdAsync(Guid id, CancellationToken cancellationToken = default)
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
            throw CustomExceptionFactory.CreateNotFoundError("Users");

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
            throw CustomExceptionFactory.CreateNotFoundError("Users");

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
            throw CustomExceptionFactory.CreateNotFoundError("Users");

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
            throw CustomExceptionFactory.CreateNotFoundError("User");

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

            var userLog = new UserLog
            {
                ActorUserId = userId,
                Title = "Update self user",
                Description = $"User {user.UserName} has updated profile."
            };
            await _userLog.CreateLog(userLog);
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
    public async Task<SelfUserResponse?> UpdateSelfUserByAdminAsync(Guid id, UpdateSelfUserRequest request, CancellationToken cancellationToken)
    {
        var adminId = _currentService.GetUserId();
        var user = await _userRepository.GetUserByIdAsync(id);
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
            var userLog = new UserLog
            {
                ActorUserId = adminId,
                Title = "Update profile",
                Description = $"Admin has updated profile {user.UserName} with id {user.Id}."
            };
            await _userLog.CreateLog(userLog);
            return result;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

    }
    public async Task<SelfUserResponse?> UpdateStatus(Guid id, bool Status, CancellationToken cancellationToken)
    {
        var adminId = _currentService.GetUserId();
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("User"));

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            user.Status = Status;

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

            var userLog = new UserLog
            {
                ActorUserId = adminId,
                Title = "Update status",
                Description = $"Admin has updated status of profile {user.UserName} with id {user.Id} / status = '{user.Status}'."
            };
            await _userLog.CreateLog(userLog);
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
            throw CustomExceptionFactory.CreateNotFoundError("User");

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
            var userLog = new UserLog
            {
                ActorUserId = user.Id,
                Title = "Change password",
                Description = $"User {user.UserName} has updated password."
            };
            await _userLog.CreateLog(userLog);
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
    public async Task<SelfUserResponse?> GetOwnerUserByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        if (companyId == Guid.Empty)
            throw CustomExceptionFactory.CreateBadRequestError(
                ResponseMessages.INVALID_INPUT.FormatMessage("Company Id"));

        // Lấy User thay vì chỉ OwnerId
        var owner = await _userRepository.GetOwnerUserByCompanyIdAsync(companyId, cancellationToken);

        if (owner == null)
            throw CustomExceptionFactory.CreateNotFoundError(
                ResponseMessages.NOT_FOUND.FormatMessage("Owner User"));

        // Map sang SelfUserResponse
        var response = _mapper.Map<SelfUserResponse>(owner);

        return response;
    }

    // ================ Overview Methods ===================
    public async Task<UserStatusResponse> GetCountUserByStatusAsync(CancellationToken cancellationToken = default)
    {
        var result = await _userRepository.GetCountUserByStatusAsync(cancellationToken);

        return new UserStatusResponse
        {
            CountFalse = result.False,
            CountTrue = result.True,
        };

    }
    public async Task<UserGrowthAndStatusOverviewResponse> GetUserGrowthAndStatusOverviewAsync(int months = 12, CancellationToken cancellationToken = default)
    {
        if (months <= 0)
        {
            months = 12;
        }

        // Giả định CreateAt đang lưu giờ VN (UTC+7) giống UpdateAt
        var now = DateTime.UtcNow.AddHours(7);

        // Lấy từ đầu tháng (months-1) trước đến hết tháng hiện tại
        var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
        var from = startOfCurrentMonth.AddMonths(-months + 1);
        var to = startOfCurrentMonth.AddMonths(1); // exclusive

        // 1) Growth: số user mới theo tháng
        var rawGrowth = await _userRepository.GetUserGrowthAsync(from, to, cancellationToken);

        // 2) Active / Inactive / Total
        var statusTuple = await _userRepository.GetCountUserByStatusAsync(cancellationToken);
        var totalUsers = await _userRepository.GetTotalUsersAsync(cancellationToken);

        // 3) Fill đầy đủ các tháng (kể cả tháng không có user mới => 0)
        var growthPoints = new List<UserGrowthPointResponse>();
        var cursor = new DateTime(from.Year, from.Month, 1);

        for (int i = 0; i < months; i++)
        {
            var monthData = rawGrowth
                .FirstOrDefault(x => x.Year == cursor.Year && x.Month == cursor.Month);

            growthPoints.Add(new UserGrowthPointResponse
            {
                Period = $"{cursor:yyyy-MM}",
                NewUsers = monthData?.Count ?? 0
            });

            cursor = cursor.AddMonths(1);
        }

        var response = new UserGrowthAndStatusOverviewResponse
        {
            Growth = growthPoints,
            TotalUsers = totalUsers,
            ActiveUsers = statusTuple.True,
            InactiveUsers = statusTuple.False
        };

        return response;
    
    }
    public async Task<List<UserCompanyDistributionPoint>> GetTopCompaniesByUserCountAsync(
      int top = 10,
      CancellationToken cancellationToken = default)
    {
        if (top <= 0) top = 10;

        var raw = await _userRepository.GetTopCompaniesByUserCountAsync(top, cancellationToken);

        var result = raw
            .Select(x => new UserCompanyDistributionPoint
            {
                CompanyId = x.CompanyId,
                CompanyName = x.CompanyName,
                UserCount = x.UserCount
            })
            .ToList();

        return result;
    }
    public async Task<UserPermissionLevelOverviewResponse> GetUserPermissionLevelOverviewAsync( CancellationToken cancellationToken = default)
    {
        var points = await _userRepository.GetUserPermissionLevelOverviewAsync(cancellationToken);

        var response = new UserPermissionLevelOverviewResponse
        {
            TotalUsers = points.Sum(p => p.Count),
            Levels = points.Select(p => new UserPermissionLevelPointResponse
            {
                Level = p.Level,
                Count = p.Count
            }).ToList()
        };

        return response;
    }

    public async Task<AnalyticsUserResponse> GetAnalyticsUserAsync(Guid userId,
 CancellationToken token)
    {
        // 1. User Performance
        var rawPerformance = await _userRepository.GetUserPerformanceOverviewAsync(userId, token);
        var userPerformance = new UserPerformanceOverview
        {
            TotalTasksAssigned = rawPerformance.TotalTasksAssigned,
            TotalCompanies = rawPerformance.TotalCompanies,
            TotalProjects = rawPerformance.TotalProjects,
            TotalSubscriptions = rawPerformance.TotalSubscriptions,
        };

        // 2. Assign To Me
        var assignTasks = await _taskRepository.GetTasksAssignedToUserAsync(userId, token);
        var assignToMe = assignTasks.Select(t => new ProjectTaskResponse
        {
            Id = t.Id,
            Type = t.Type,
            Code = t.Code,
            Title = t.Title,
            CreateAt = t.CreateAt,
            UpdateAt = t.UpdateAt,
            DueDate = t.DueDate,
            
        }).ToList();

        // 3. Dashboard
        var dashboardData = await _taskRepository.GetUserTaskDashboardAsync(userId, token);
        var dashboard = new UserTaskDashBoard
        {
            BugPercent = dashboardData.BugPercent,
            FeaturePercent = dashboardData.FeaturePercent,
            ChorePercent = dashboardData.ChorePercent,
            OverduePercent = dashboardData.OverduePercent,
            OnTimePercent = dashboardData.OnTimePercent,
            EarlyCompletedPercent = dashboardData.EarlyCompletedPercent
        };

        // Compose response
        var response = new AnalyticsUserResponse
        {
            UserPerformance = userPerformance,
            AssignToMe = assignToMe,
            Dashboard = dashboard
        };

        return response;
    }
}
