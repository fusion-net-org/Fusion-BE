
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.User;
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
    }
}
