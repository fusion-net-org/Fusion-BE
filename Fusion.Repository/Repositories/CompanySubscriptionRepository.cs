

using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.CompanySubscriptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace Fusion.Repository.Repositories
{
    public class CompanySubscriptionRepository : GenericRepository<CompanySubscription>, ICompanySubscriptionRepository
    {
        private readonly FusionDbContext _context;
        private readonly IUserSubscriptionRepository _userSubscriptionRepository;
        public CompanySubscriptionRepository(FusionDbContext context, IUserSubscriptionRepository userSubscriptionRepository) : base(context)
        {
            _context = context;
            _userSubscriptionRepository = userSubscriptionRepository;
        }

        public async Task<CompanySubscription> CreateAsync(CompanySubscription companySubscription, CancellationToken cancellationToken = default)
        {
            // 1. check user subscription 
            var userSub = await _userSubscriptionRepository.GetByIdWithNavAsync(companySubscription.UserSubscriptionId);

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 2.validate entitlement list
                if (companySubscription.CompanySubscriptionEntitlements == null || !companySubscription.CompanySubscriptionEntitlements.Any())
                    throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.BAD_REQUEST.FormatMessage("At least one entitlement is required"));

                // 3. Validate & different quota of userSubscription
                await _userSubscriptionRepository.ValidateAndConsumeEntitlementsAsync(
                    companySubscription.UserSubscriptionId,
                    companySubscription.CompanySubscriptionEntitlements,
                    cancellationToken);

                // 4.Assign information inherited from user subscription

                companySubscription.NameSubscription ??= userSub.NamePlan;
                companySubscription.CreatedAt = DateTime.UtcNow;
                companySubscription.UpdatedAt = null;
                companySubscription.Status = SubscriptionStatus.Active;
                companySubscription.ExpiredAt = userSub.ExpiredAt;

                // 5. Assign information Entitlements
                foreach (var ent in companySubscription.CompanySubscriptionEntitlements)
                {
                    ent.Id = default;
                    ent.CompanySubscriptionId = companySubscription.Id;
                    ent.Remaining = ent.Quantity;
                }

                // 6. Assign information company subscription role 
                if (companySubscription.CompanySubscriptionRoles != null && companySubscription.CompanySubscriptionRoles.Any())
                {
                    foreach (var role in companySubscription.CompanySubscriptionRoles)
                    {
                        role.Id = default;
                        role.CompanySubscriptionId = companySubscription.Id;
                    }
                }

                _context.CompanySubscriptions.Add(companySubscription);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return companySubscription;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResult<CompanySubscription>> GetAllAsync(CompanySubscriptionPagedRequest request, CancellationToken ct = default)
        {
            var q = _context.CompanySubscriptions
                            .AsNoTracking()
                            .Include(cs => cs.Company)
                            .Include(cs => cs.UserSubscription)
                            .Include(cs => cs.CompanySubscriptionEntitlements)
                            .Include(cs => cs.CompanySubscriptionRoles)
                            .AsQueryable();

            // --- Lọc theo tên gói ---
            if (!string.IsNullOrWhiteSpace(request.PlanName))
            {
                var pattern = $"%{request.PlanName.Trim()}%";
                q = q.Where(cs => cs.NameSubscription != null && EF.Functions.Like(cs.NameSubscription, pattern));
            }

            // --- Lọc theo trạng thái ---

            if (request.status.HasValue)
            {
                q = q.Where(cs => cs.UserSubscription != null && cs.UserSubscription.Status == request.status.Value);
            }

            // --- Lọc theo thời gian tạo ---
            if (request.CreateAt.From.HasValue)
                q = q.Where(cs => cs.CreatedAt >= request.CreateAt.From.Value);

            if (request.CreateAt.To.HasValue)
                q = q.Where(cs => cs.CreatedAt <= request.CreateAt.To.Value);

            // --- Lọc theo thời gian hết hạn ---
            if (request.ExpiredAt.From.HasValue)
                q = q.Where(cs => cs.ExpiredAt >= request.ExpiredAt.From.Value);

            if (request.ExpiredAt.To.HasValue)
                q = q.Where(cs => cs.ExpiredAt <= request.ExpiredAt.To.Value);

            // --- Tìm kiếm tổng hợp ---
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var kw = $"%{request.Keyword.Trim()}%";
                q = q.Where(cs =>
                    (cs.NameSubscription != null && EF.Functions.Like(cs.NameSubscription, kw)) ||
                    (cs.UserSubscription != null && EF.Functions.Like(cs.UserSubscription.NamePlan!, kw))
                );
            }

            // --- Sắp xếp ---
            string sortColumn = nameof(CompanySubscription.CreatedAt);
            bool sortDescending = true;

            if (!string.IsNullOrWhiteSpace(request.SortColumn) && CompanySubscriptionPagedRequest.SortMap.TryGetValue(request.SortColumn, out var mapped))
                sortColumn = mapped;
            else
                sortColumn = nameof(CompanySubscription.CreatedAt);

            sortDescending = request.SortDescending;

            // --- Phân trang ---
            return await q.ToPagedResultAsync(request, ct);
        }

        public async Task<PagedResult<CompanySubscription>> GetAllByCompanyIdAsync(Guid companyId, CompanySubscriptionPagedRequest request, CancellationToken ct = default)
        {
            var q = _context.CompanySubscriptions
                           .AsNoTracking()
                           .Include(cs => cs.Company)
                           .Where(cs => cs.Company.Id == companyId)
                           .Include(cs => cs.UserSubscription)
                           .Include(cs => cs.CompanySubscriptionEntitlements)
                           .Include(cs => cs.CompanySubscriptionRoles)
                           .AsQueryable();

            // --- Lọc theo tên gói ---
            if (!string.IsNullOrWhiteSpace(request.PlanName))
            {
                var pattern = $"%{request.PlanName.Trim()}%";
                q = q.Where(cs => cs.NameSubscription != null && EF.Functions.Like(cs.NameSubscription, pattern));
            }

            // --- Lọc theo trạng thái ---

            if (request.status.HasValue)
            {
                q = q.Where(cs => cs.UserSubscription != null && cs.UserSubscription.Status == request.status.Value);
            }

            // --- Lọc theo thời gian tạo ---
            if (request.CreateAt.From.HasValue)
                q = q.Where(cs => cs.CreatedAt >= request.CreateAt.From.Value);

            if (request.CreateAt.To.HasValue)
                q = q.Where(cs => cs.CreatedAt <= request.CreateAt.To.Value);

            // --- Lọc theo thời gian hết hạn ---
            if (request.ExpiredAt.From.HasValue)
                q = q.Where(cs => cs.ExpiredAt >= request.ExpiredAt.From.Value);

            if (request.ExpiredAt.To.HasValue)
                q = q.Where(cs => cs.ExpiredAt <= request.ExpiredAt.To.Value);

            // --- Tìm kiếm tổng hợp ---
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var kw = $"%{request.Keyword.Trim()}%";
                q = q.Where(cs =>
                    (cs.NameSubscription != null && EF.Functions.Like(cs.NameSubscription, kw)) ||
                    (cs.UserSubscription != null && EF.Functions.Like(cs.UserSubscription.NamePlan!, kw))
                );
            }

            // --- Sắp xếp ---
            string sortColumn = nameof(CompanySubscription.CreatedAt);
            bool sortDescending = true;

            if (!string.IsNullOrWhiteSpace(request.SortColumn) && CompanySubscriptionPagedRequest.SortMap.TryGetValue(request.SortColumn, out var mapped))
                sortColumn = mapped;
            else
                sortColumn = nameof(CompanySubscription.CreatedAt);

            sortDescending = request.SortDescending;

            // --- Phân trang ---
            return await q.ToPagedResultAsync(request, ct);
        }

        public Task<CompanySubscription?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default)
            => _context.CompanySubscriptions
                .Include(cs => cs.Company)
                .Include(cs => cs.UserSubscription)
                .Include(cs => cs.CompanySubscriptionEntitlements)
                .Include(cs => cs.CompanySubscriptionRoles)
                .FirstOrDefaultAsync(cs => cs.Id == id, ct);

        public Task<CompanySubscription> UpdateAsync(Guid id, CompanySubscription update, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<CompanySubscription> UpdateStatusAsync(Guid id, SubscriptionStatus status, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
