

using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.CompanySubscriptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class CompanySubscriptionRepository : GenericRepository<CompanySubscription>, ICompanySubscriptionRepository
    {
        private readonly FusionDbContext _context;
        private readonly IUserSubscriptionRepository _userSubscriptionRepository;
        private readonly ICompanySubscriptionEntryRepository _entry;
        public CompanySubscriptionRepository(FusionDbContext context, IUserSubscriptionRepository userSubscriptionRepository
            , ICompanySubscriptionEntryRepository entry) : base(context)
        {
            _context = context;
            _userSubscriptionRepository = userSubscriptionRepository;
            _entry = entry;
        }
        public async Task<CompanySubscription> CreateAsync(CompanySubscription companySubscription, CancellationToken cancellationToken = default)
        {
            // 1. Load UserSubscription + Entitlements
            var userSub = await _userSubscriptionRepository
                .GetByIdWithNavAsync(companySubscription.UserSubscriptionId, cancellationToken);

            if (userSub == null)
                throw CustomExceptionFactory.CreateNotFoundError("User subscription not found");

            if (userSub.Status != SubscriptionStatus.Active)
                throw CustomExceptionFactory.CreateBadRequestError("User subscription is not active.");

            if (userSub.TermEnd.HasValue && userSub.TermEnd.Value < DateTimeOffset.UtcNow)
                throw CustomExceptionFactory.CreateBadRequestError("User subscription has expired.");

            // 2. Check share limit
            if (userSub.CompanyShareLimitSnapshot.HasValue)
            {
                if (userSub.CompanyShareLimitSnapshot.Value <= 0)
                    throw CustomExceptionFactory.CreateBadRequestError("No remaining company shares for this subscription.");
            }

            // 3. Kiểm tra share trùng
            var exists = await _context.CompanySubscriptions
                .AnyAsync(cs =>
                    cs.UserSubscriptionId == companySubscription.UserSubscriptionId &&
                    cs.CompanyId == companySubscription.CompanyId,
                    cancellationToken);

            if (exists)
                throw CustomExceptionFactory.CreateBadRequestError("This subscription has already been shared to the specified company.");

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 4. Trừ hạn mức share nếu có
                await _userSubscriptionRepository.DecreaseCompanyShareLimitAsync(userSub.Id, 1, cancellationToken);

                // 5. Gán thông tin CompanySubscription từ UserSubscription
                companySubscription.Status = SubscriptionStatus.Active;
                companySubscription.SharedOn = DateTimeOffset.UtcNow;
                companySubscription.UpdatedAt = DateTimeOffset.UtcNow;
                companySubscription.ExpiredAt = userSub.TermEnd;

                // Nếu OwnerUserId chưa set (Guid.Empty) thì dùng chủ sở hữu userSub
                if (companySubscription.OwnerUserId == Guid.Empty)
                {
                    companySubscription.OwnerUserId = userSub.UserId;
                }

                companySubscription.SeatsLimitSnapshot = userSub.SeatsPerCompanyLimitSnapshot;
                // SeatsLimitUnit hiện chưa dùng, để null

                await _context.CompanySubscriptions.AddAsync(companySubscription, cancellationToken);

                //  Copy entitlements từ UserSubscription -> CompanySubscription
                var userEntitlements = userSub.Entitlements
                    .Where(e => e.Enabled)
                    .ToList();

                if (userEntitlements.Count > 0)
                {
                    var companyEntitlements = userEntitlements.Select(e => new CompanySubscriptionEntitlement
                    {
                        CompanySubscriptionId = companySubscription.Id,
                        FeatureId = e.FeatureId,
                        Enabled = true
                    }).ToList();

                    await _context.CompanySubscriptionEntitlements
                        .AddRangeAsync(companyEntitlements, cancellationToken);
                }

                // 6. Save & Commit
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
        public async Task<List<CompanySubscription>> GetAllActiveByCompanyIdAsync(Guid companyId, CancellationToken ct = default)
        {
            return await _context.CompanySubscriptions
                     .AsNoTracking()
        .Include(cs => cs.UserSubscription)
            .ThenInclude(us => us.Plan)
        .Include(cs => cs.Entitlements
            .Where(e => e.Feature.Category != "User"))
            .ThenInclude(e => e.Feature)
        .Where(cs => cs.CompanyId == companyId &&
                     cs.Status == SubscriptionStatus.Active)
        .ToListAsync(ct);
        }
        public async Task<PagedResult<CompanySubscription>> GetAllByCompanyIdAsync(Guid companyId, CompanySubscriptionPagedRequest request, CancellationToken ct = default)
        {
            var q = _context.CompanySubscriptions
                            .AsNoTracking()
                            .Include(cs => cs.Company)
                            .Include(cs => cs.UserSubscription)
                              .ThenInclude(us => us.Plan)

                            .Include(cs => cs.UserSubscription)
                              .ThenInclude(us => us.User)
                            .Where(cs => cs.CompanyId == companyId);


            if (request.Status.HasValue)
            {
                q = q.Where(x => x.Status == request.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var kw = request.Keyword.Trim();
                var like = $"%{kw}%";

                q = q.Where(x =>
                    (x.Company.Name != null && EF.Functions.Like(x.Company.Name, like)) ||
                    (x.UserSubscription.Plan.Name != null && EF.Functions.Like(x.UserSubscription.Plan.Name, like)));
            }

            if (!string.IsNullOrWhiteSpace(request.SortColumn) &&
                CompanySubscriptionPagedRequest.SortMap.TryGetValue(request.SortColumn, out var mapped))
            {
                request.SortColumn = mapped;
            }
            else if (string.IsNullOrWhiteSpace(request.SortColumn))
            {
                request.SortColumn = nameof(CompanySubscription.SharedOn);
                request.SortDescending = true;
            }

            return await q.ToPagedResultAsync(request, ct);

        }
        public async Task<CompanySubscription?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.CompanySubscriptions
                .AsNoTracking()
                .Include(cs => cs.Company)
                .Include(cs => cs.UserSubscription)
                    .ThenInclude(us => us.Plan)
                .Include(cs => cs.UserSubscription)
                    .ThenInclude(us => us.User)
                .Include(cs => cs.Entitlements
                    .Where(e => e.Feature.Category != "User"))
                    .ThenInclude(e => e.Feature)
                .FirstOrDefaultAsync(cs => cs.Id == id, ct);
        }
        public async Task<int> UpdateEnabledByFeatureIdAsync(Guid featureId, bool newStatus, CancellationToken ct = default)
        {
            var ents = await _context.CompanySubscriptionEntitlements
                       .Where(e => e.FeatureId == featureId)
                       .ToListAsync(ct);

            if (ents.Count == 0)
                return 0;

            foreach (var e in ents)
            {
                if (e.Enabled != newStatus)
                {
                    e.Enabled = newStatus;
                }
            }


            return ents.Count;
        }
        public async Task UseFeatureAsync(Guid companySubscriptionId, long companyMemberId, string featureName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(featureName))
                throw CustomExceptionFactory.CreateBadRequestError("Feature code is required.");

            featureName = featureName.Trim();

            // 1. Load subscription + entitlements + feature để so sánh theo code
            var sub = await _context.CompanySubscriptions
                .Include(cs => cs.Entitlements)
                    .ThenInclude(e => e.Feature)
                .FirstOrDefaultAsync(cs => cs.Id == companySubscriptionId, ct);

            if (sub == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Company subscription."));

            if (sub.Status != SubscriptionStatus.Active)
                throw CustomExceptionFactory.CreateBadRequestError(
                    "Company subscription is not active.");

            if (sub.ExpiredAt.HasValue && sub.ExpiredAt.Value < DateTimeOffset.UtcNow)
                throw CustomExceptionFactory.CreateBadRequestError(
                    "Company subscription has expired.");

            // 2. Tìm entitlement theo Feature.Code (thay vì FeatureId)
            var entitlement = sub.Entitlements
                .FirstOrDefault(e =>
                    e.Enabled &&
                    e.Feature != null &&
                    e.Feature.Name != null &&
                    e.Feature.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase));

            if (entitlement == null)
                throw CustomExceptionFactory.CreateBadRequestError(
         $"Feature '{featureName}' is not enabled for the selected company subscription.");

            // 3. Ghi nhận user dùng gói (consume seat)
            //    Hàm CreateAsync bên CompanySubscriptionEntryRepository sẽ:
            //    - Không double count nếu member đã có entry
            //    - Check SeatsLimitSnapshot / SeatsLimitUnit
            //    - Tạo entry + tăng SeatsLimitUnit nếu cần
            await _entry.CreateAsync(companySubscriptionId, companyMemberId, ct);
        }
    }
}
