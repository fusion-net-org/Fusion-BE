
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels.Users;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly FusionDbContext _context;

        public UserRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }
        public async Task<List<RoleDto>> GetRolesByUserAndCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.Role != null && ur.Role.CompanyId == companyId)
                .Select(ur => new RoleDto
                {
                    Id = ur.Role!.Id,
                    Name = ur.Role.RoleName!,
                    Description = ur.Role.Description
                })
                .ToListAsync(cancellationToken);
        }



        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }
        public async Task<User?> GetUserByGoogleSubAsync(string googleSub, CancellationToken cancellationToken = default)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.GoogleSub == googleSub, cancellationToken);
        }
        public async Task<bool> CheckEmailExistAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
        }
        public async Task<PagedResult<User>> GetPagedAdminUsersAsync(AdminUserPagedRequest request, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            // search
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                query = query.Where(u => (u.Email ?? "").Contains(request.Email));
            }

            if (!string.IsNullOrWhiteSpace(request.Company))
            {
                query = query.Where(u =>
                    u.CompanyMembers.Any(cm =>
                        cm.Company != null &&
                        (cm.Company.Name ?? "").Contains(request.Company))
                );
            }

            if (request.Stauts.HasValue)
            {
                query = query.Where(u => u.Status == request.Stauts);
            }

            // dùng extension để phân trang + sort
            return await query.ToPagedResultAsync(request, cancellationToken);
        }
        public async Task<PagedResult<User>> GetPagedCompanyUsersAsync(CompanyUserPagedRequest request, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            // search
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                query = query.Where(u => (u.Email ?? "").Contains(request.Email));
            }
            // dùng extension để phân trang + sort
            return await query.ToPagedResultAsync(request, cancellationToken);
        }
        public async Task<PagedResult<User>> GetAllUsersAsync(PagedRequest request, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();


            return await query.ToPagedResultAsync(request, cancellationToken);
        }
        public async Task<User?> GetOwnerUserByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
        {
            var ownerUserId = await _context.Companies
                .AsNoTracking()
                .Where(c => c.Id == companyId)
                .Select(c => c.OwnerUserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (ownerUserId == null)
                return null;

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == ownerUserId, cancellationToken);

            return user;
        }
        public async Task<User?> GetUserByResetTokenAsync(string resetToken, CancellationToken cancellationToken = default)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == resetToken, cancellationToken);
        }
        public async Task<User?> GetUserWithRolesAndPermissionsInCompanyAsync(Guid userId, Guid companyId)
        {
            return await _context.Users.Include(u => u.UserRoles.Where(ur => ur.Role.CompanyId == companyId))
                        .ThenInclude(ur => ur.Role)
                            .ThenInclude(r => r.RolePermissions.Where(rp => rp.CompanyId == companyId))
                                    .ThenInclude(rp => rp.Function).FirstOrDefaultAsync(u => u.Id == userId);
        }
        public Task<int> GetAllUserAsync(CancellationToken cancellationToken = default)
        {
            return _context.Users
                .AsNoTracking()
                .CountAsync(cancellationToken);
        }
        public async Task<(int False, int True)> GetCountUserByStatusAsync(CancellationToken cancellationToken = default)
        {
            var row = await _context.Users
                .AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    False = g.Count(u => u.Status == false),
                    True = g.Count(u => u.Status == true)
                })
                .FirstOrDefaultAsync(cancellationToken);

            return (row?.False ?? 0, row?.True ?? 0);
        }
        public async Task<bool> EmailVerificationAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token, cancellationToken);
            if (user == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Token"));

            var nowUtc = DateTime.UtcNow;

            if (!user.Status)
            {
                user.Status = true;
                user.UpdateAt = nowUtc.AddHours(7);
            }
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        // ================================== OverView  ================================== 
        public Task<int> GetTotalUsersAsync(CancellationToken cancellationToken = default)
        {
            // bạn có thể dùng luôn method này cho các chỗ cần tổng user
            return _context.Users
                .AsNoTracking()
                .CountAsync(cancellationToken);
        }
        public async Task<List<GetUserGrowth>> GetUserGrowthAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            if (from.HasValue)
            {
                query = query.Where(u => u.CreateAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(u => u.CreateAt < to.Value);
            }

            var data = await query
                .GroupBy(u => new { u.CreateAt.Year, u.CreateAt.Month })
                .Select(g => new GetUserGrowth
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync(cancellationToken);

            return data;
        }
        public async Task<List<UserCompanyDistributionPoint>> GetTopCompaniesByUserCountAsync(int top, CancellationToken cancellationToken = default)
        {
            if (top <= 0) top = 10;

            var data = await _context.CompanyMembers
                .AsNoTracking()
                .Where(cm => cm.Company != null)
                .GroupBy(cm => new
                {
                    cm.CompanyId,
                    CompanyName = cm.Company!.Name
                })
                .Select(g => new UserCompanyDistributionPoint
                {
                    CompanyId = g.Key.CompanyId,
                    CompanyName = g.Key.CompanyName ?? "Unknown",
                    UserCount = g
                        .Select(x => x.UserId)   // distinct user per company
                        .Distinct()
                        .Count()
                })
                .OrderByDescending(x => x.UserCount)
                .ThenBy(x => x.CompanyName)
                .Take(top)
                .ToListAsync(cancellationToken);

            return data;
        }

        public async Task<List<UserPermissionLevelPoint>> GetUserPermissionLevelOverviewAsync(
            CancellationToken cancellationToken = default)
        {
            var query = _context.Users
                .AsNoTracking()
                .Select(u => new
                {
                    Level =
                        u.IsSystemAdmin
                            ? "System admin"
                            : u.Companies.Any()
                                ? "Company owner"
                                : u.CompanyMembers.Any()
                                    ? "Company member"
                                    : "Registered only"
                });

            var grouped = await query
                .GroupBy(x => x.Level)
                .Select(g => new UserPermissionLevelPoint
                {
                    Level = g.Key,
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);

            // Bảo đảm đủ 4 bucket, kể cả = 0
            var orderedLevels = new[]
            {
            "System admin",
            "Company owner",
            "Company member",
            "Registered only"
        };

            var dict = grouped.ToDictionary(x => x.Level, StringComparer.OrdinalIgnoreCase);

            var result = orderedLevels
                .Select(level =>
                    dict.TryGetValue(level, out var v)
                        ? v
                        : new UserPermissionLevelPoint { Level = level, Count = 0 })
                .ToList();

            return result;
        }

        public async Task<List<UserMonthlyNewPoint>> GetMonthlyNewUsersInYearAsync(int year, CancellationToken ct = default)
        {
            return await _context.Users
      .AsNoTracking()
      .Where(u => u.CreateAt.Year == year)
      .GroupBy(u => new { u.CreateAt.Year, u.CreateAt.Month })
      .Select(g => new UserMonthlyNewPoint
      {
          Year = g.Key.Year,
          Month = g.Key.Month,
          NewUsers = g.Count()
      })
      .ToListAsync(ct);
        }

        public async Task<UserPerformanceOverview> GetUserPerformanceOverviewAsync(Guid userId, CancellationToken token = default)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                throw CustomExceptionFactory.CreateNotFoundError("User Not existed");

            var taskIds = await _context.TaskWorkflows
                .Where(a => a.AssignUserId == userId)
                .Select(a => a.TaskId)
                .ToListAsync();

            var totalTasksAssigned = await _context.ProjectTasks
                .Include(t => t.TaskWorkflows)
                .Where(t => !t.IsDeleted && taskIds.Contains(t.Id))
                .CountAsync(token);

            var totalCompanies = await _context.CompanyMembers
                .Where(cm => cm.UserId == userId)
                .Select(cm => cm.CompanyId)
                .Distinct()
                .CountAsync(token);

            var totalProjects = await _context.ProjectMembers
                .Where(cm => cm.UserId == userId)
                .Select(cm => cm.ProjectId)
                .Distinct()
                .CountAsync(token);

            var totalSubscriptions = await _context.UserSubscriptions
            .Where(us => us.UserId == userId && us.Status == Enums.SubscriptionStatus.Active)
            .CountAsync(token);

            return new UserPerformanceOverview
            {
                TotalTasksAssigned = totalTasksAssigned,
                TotalCompanies = totalCompanies,
                TotalProjects = totalProjects,
                TotalSubscriptions = totalSubscriptions

            };
        }
    }
}
