
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
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


            var subscription = await _context.SubscriptionPackages
                             .AsNoTracking()
                             .FirstOrDefaultAsync(x => x.Price == 0, cancellationToken);

            if (subscription == null)
                throw CustomExceptionFactory.CreateNotFoundError("Subscription package 'NewMember' Not exsit. Let seed before.");

            var alreadyGranted = await _context.UserSubscriptions
                                 .AsNoTracking()
                                 .AnyAsync(x => x.UserId == user.Id && x.PackageId == subscription.Id, cancellationToken);


            if (!alreadyGranted)
            {
                var grant = new UserSubscription
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    PackageId = subscription.Id,
                    PurchaseDate = nowUtc,

                    // Lấy quota từ package
                    QuotaCompanyAdded = subscription.QuotaCompany,
                    QuotaProjectAdded = subscription.QuotaProject,
                    QuotaCompanyRemaining = subscription.QuotaCompany,
                    QuotaProjectRemaining = subscription.QuotaProject,

                    ExpiryDate = null,
                    IsActive = true
                };
                _context.UserSubscriptions.Add(grant);
            }
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
