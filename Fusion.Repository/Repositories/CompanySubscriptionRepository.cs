

//using Fusion.Repository.Bases.Exceptions;
//using Fusion.Repository.Bases.Page;
//using Fusion.Repository.Bases.Page.CompanySubscriptions;
//using Fusion.Repository.Bases.Responses;
//using Fusion.Repository.Data;
//using Fusion.Repository.Entities;
//using Fusion.Repository.Enums;
//using Fusion.Repository.IRepositories;
//using Microsoft.EntityFrameworkCore;

//namespace Fusion.Repository.Repositories
//{
//    public class CompanySubscriptionRepository : GenericRepository<CompanySubscription>, ICompanySubscriptionRepository
//    {
//        private readonly FusionDbContext _context;
//        private readonly IUserSubscriptionRepository _userSubscriptionRepository;
//        public CompanySubscriptionRepository(FusionDbContext context, IUserSubscriptionRepository userSubscriptionRepository) : base(context)
//        {
//            _context = context;
//            _userSubscriptionRepository = userSubscriptionRepository;
//        }

//        public async Task<CompanySubscription> UpdateAsync(Guid userId, CompanySubscription update, CancellationToken ct = default)
//        {
//            var comanySub = await GetByIdWithNavAsync(update.Id);
//            if (comanySub == null)
//                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company Subscription"));

//            var userSub = await _context.UserSubscriptions
//                 .Include(u => u.UserSubscriptionEntitlements)
//                 .FirstOrDefaultAsync(us => us.Id == comanySub.UserSubscriptionId);
//            if (userSub == null)
//                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.BAD_REQUEST.FormatMessage("User subscription"));

//            var transaction = await _context.TransactionPayments
//                .FirstOrDefaultAsync(x => x.Id == userSub.TransactionId);
//            if (transaction == null)
//                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Transaction."));

//            if (userId != transaction.UserId)
//                throw CustomExceptionFactory.CreateForbiddenError();

//            using var tx = await _context.Database.BeginTransactionAsync(ct);
//            try
//            {
//                // Update status
//                if (update.Status != comanySub.Status)
//                    comanySub.Status = update.Status;

//                // Normalize incoming entilements list
//                var incoming = (update.CompanySubscriptionEntitlements ?? new List<CompanySubscriptionEntitlement>())
//                              .ToList();

//                //  ====================== Handle removals  ======================
//                // any existing entiliement that us not present in incoming list should be removed
//                // when removing, refund its Quantitty back to user's remaing 
//                var incomingIds = incoming.Where(i => i.Id != default).Select(i => i.Id).ToHashSet();
//                var toRemove = comanySub.CompanySubscriptionEntitlements
//                    .Where(e => e.Id != default && !incomingIds.Contains(e.Id))
//                    .ToList();

//                foreach (var rem in toRemove)
//                {
//                    // find corresponging user entitlement by FeatureKey
//                    var useEnt = userSub.UserSubscriptionEntitlements.FirstOrDefault(u => u.FeatureKey == rem.FeatureKey);
//                    if (useEnt == null)
//                        throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Feature Key of user subscription"));

//                    // refund the entire quatity back to user's reaining 
//                    useEnt.Remaining += rem.Quantity;

//                    // Remove from existing list
//                    _context.CompanySubscriptionEntitlements.Remove(rem);
//                }


//                // ===============Hand update amd mew emtilements ==========
//                foreach (var inc in incoming)
//                {

//                    // if incoming item has an Id and matches and existing entitlement => update
//                    CompanySubscriptionEntitlement? existingEnt = null;
//                    if (inc.Id != default)
//                    {
//                        existingEnt = comanySub.CompanySubscriptionEntitlements.FirstOrDefault(e => e.Id == inc.Id);
//                    }
//                    else
//                    {
//                        // if no id, try match by FeatureKey
//                        existingEnt = comanySub.CompanySubscriptionEntitlements.FirstOrDefault(e => e.FeatureKey == inc.FeatureKey);
//                    }

//                    if (existingEnt != null)
//                    {
//                        //Existing entitlement -> may change quantity up or down
//                        var newQty = inc.Quantity;
//                        if (newQty < 0)
//                            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.BAD_REQUEST.FormatMessage("Quantity must be non-negative"));

//                        var olQuy = existingEnt.Quantity;

//                        var diff = newQty - olQuy;

//                        // find the corresponding user entitlement 
//                        var userEnt = userSub.UserSubscriptionEntitlements.FirstOrDefault(u => u.FeatureKey == existingEnt.FeatureKey);
//                        if (userEnt == null)
//                            throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage($"Feature {existingEnt.FeatureKey} not found in user subscription"));

//                        if (diff > 0)
//                        {

//                            // Need to cosume additional quota from user subscription
//                            if (userEnt.Remaining < diff)
//                                throw CustomExceptionFactory.CreateBadRequestError(
//                                    ResponseMessages.BAD_REQUEST.FormatMessage($"Not enough quota for feature {existingEnt.FeatureKey}"));

//                            userEnt.Remaining -= diff;
//                            existingEnt.Quantity = newQty;
//                            existingEnt.Remaining += diff;  // company gets additional remaining quota
//                        }

//                        else if (diff < 0)
//                        {
//                            // Decease company quota -> refund to user subscription
//                            var refund = -diff;

//                            // check if reaminng of company < diff
//                            var used = existingEnt.Quantity - existingEnt.Remaining;
//                            if (refund > existingEnt.Remaining)
//                                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.BAD_REQUEST.FormatMessage(
//                                    $"Cannot reduce quota for feature {existingEnt.FeatureKey} below the amount already used ({used})."));

//                            // update company quota
//                            existingEnt.Quantity = newQty;
//                            existingEnt.Remaining -= refund;
//                            userEnt.Remaining += refund;

//                            // If remaining < 0, fix
//                            if (existingEnt.Remaining < 0)
//                                existingEnt.Remaining = 0;
//                        }

//                        _context.CompanySubscriptionEntitlements.Update(existingEnt);
//                    }
//                    else
//                    {
//                        // new entitlement being added for the copany
//                        var feature = inc.FeatureKey;
//                        var qty = inc.Quantity;

//                        if (qty <= 0)
//                            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.BAD_REQUEST.FormatMessage("Quantity must be greater than 0 for new entitlement"));

//                        var userEnt = userSub.UserSubscriptionEntitlements.FirstOrDefault(u => u.FeatureKey == feature);
//                        if (userEnt == null)
//                            throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage($"Feature {feature} not found in user subscription"));

//                        if (userEnt.Remaining < qty)
//                            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.BAD_REQUEST.FormatMessage($"Not enough quota for feature {feature}"));

//                        // Consume user's quota
//                        userEnt.Remaining -= qty;

//                        // Create new company entitlement
//                        var newCompanyEnt = new CompanySubscriptionEntitlement
//                        {
//                            Id = default,
//                            CompanySubscriptionId = comanySub.Id,
//                            FeatureKey = feature,
//                            Quantity = qty,
//                            Remaining = qty
//                        };
//                        await _context.CompanySubscriptionEntitlements.AddAsync(newCompanyEnt, ct);
//                        //comanySub.CompanySubscriptionEntitlements.Add(newCompanyEnt);
//                    }
//                }

//                comanySub.UpdatedAt = DateTime.UtcNow;
//                _context.UserSubscriptionEntitlements.UpdateRange(userSub.UserSubscriptionEntitlements);
//                _context.CompanySubscriptions.Update(comanySub);
//                await _context.SaveChangesAsync(ct);
//                await tx.CommitAsync();

//                var result = await GetByIdWithNavAsync(comanySub.Id);
//                return result;

//            }
//            catch
//            {
//                await tx.RollbackAsync();
//                throw;
//            }
//        }
//        public async Task<CompanySubscription> CreateAsync(CompanySubscription companySubscription, CancellationToken cancellationToken = default)
//        {
//            // 1. check user subscription 
//            var userSub = await _userSubscriptionRepository.GetByIdWithNavAsync(companySubscription.UserSubscriptionId);

//            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
//            try
//            {
//                // 2.validate entitlement list
//                if (companySubscription.CompanySubscriptionEntitlements == null || !companySubscription.CompanySubscriptionEntitlements.Any())
//                    throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.BAD_REQUEST.FormatMessage("At least one entitlement is required"));

//                // 3. Validate & different quota of userSubscription
//                await _userSubscriptionRepository.ValidateAndConsumeEntitlementsAsync(
//                    companySubscription.UserSubscriptionId,
//                    companySubscription.CompanySubscriptionEntitlements,
//                    cancellationToken);

//                // 4.Assign information inherited from user subscription

//                companySubscription.NameSubscription ??= userSub.NamePlan;
//                companySubscription.CreatedAt = DateTime.UtcNow;
//                companySubscription.UpdatedAt = null;
//                companySubscription.Status = SubscriptionStatus.Active;
//                companySubscription.ExpiredAt = userSub.ExpiredAt;

//                // 5. Assign information Entitlements
//                foreach (var ent in companySubscription.CompanySubscriptionEntitlements)
//                {
//                    ent.Id = default;
//                    ent.CompanySubscriptionId = companySubscription.Id;
//                    ent.Remaining = ent.Quantity;
//                }
//                _context.CompanySubscriptions.Add(companySubscription);
//                await _context.SaveChangesAsync(cancellationToken);
//                await transaction.CommitAsync(cancellationToken);

//                return companySubscription;
//            }
//            catch
//            {
//                await transaction.RollbackAsync(cancellationToken);
//                throw;
//            }

//        }
//        public async Task<PagedResult<CompanySubscription>> GetAllAsync(CompanySubscriptionPagedRequest request, CancellationToken ct = default)
//        {
//            var q = _context.CompanySubscriptions
//                            .AsNoTracking()
//                            .Include(cs => cs.Company)
//                            .Include(cs => cs.UserSubscription)
//                            .Include(cs => cs.CompanySubscriptionEntitlements)
//                            .AsQueryable();

//            // --- Lọc theo trạng thái ---

//            if (request.status.HasValue)
//            {
//                q = q.Where(cs => cs.UserSubscription != null && cs.UserSubscription.Status == request.status.Value);
//            }

//            // --- Tìm kiếm tổng hợp ---
//            if (!string.IsNullOrWhiteSpace(request.Keyword))
//            {
//                var kw = $"%{request.Keyword.Trim()}%";
//                q = q.Where(cs =>
//                    (cs.NameSubscription != null && EF.Functions.Like(cs.NameSubscription, kw)) ||
//                    (cs.UserSubscription != null && EF.Functions.Like(cs.UserSubscription.NamePlan!, kw))
//                );
//            }

//            // --- Sắp xếp ---
//            string sortColumn = nameof(CompanySubscription.CreatedAt);
//            bool sortDescending = true;

//            if (!string.IsNullOrWhiteSpace(request.SortColumn) && CompanySubscriptionPagedRequest.SortMap.TryGetValue(request.SortColumn, out var mapped))
//                sortColumn = mapped;
//            else
//                sortColumn = nameof(CompanySubscription.CreatedAt);

//            sortDescending = request.SortDescending;

//            // --- Phân trang ---
//            return await q.ToPagedResultAsync(request, ct);
//        }
//        public async Task<PagedResult<CompanySubscription>> GetAllByCompanyIdAsync(Guid companyId, CompanySubscriptionPagedRequest request, CancellationToken ct = default)
//        {
//            var q = _context.CompanySubscriptions
//                           .AsNoTracking()
//                           .Include(cs => cs.Company)
//                           .Where(cs => cs.Company.Id == companyId)
//                           .Include(cs => cs.UserSubscription)
//                           .Include(cs => cs.CompanySubscriptionEntitlements)
//                           .AsQueryable();

//            // --- Lọc theo trạng thái ---

//            if (request.status.HasValue)
//            {
//                q = q.Where(cs => cs.UserSubscription != null && cs.UserSubscription.Status == request.status.Value);
//            }

//            // --- Tìm kiếm tổng hợp ---
//            if (!string.IsNullOrWhiteSpace(request.Keyword))
//            {
//                var kw = $"%{request.Keyword.Trim()}%";
//                q = q.Where(cs =>
//                    (cs.NameSubscription != null && EF.Functions.Like(cs.NameSubscription, kw)) ||
//                    (cs.UserSubscription != null && EF.Functions.Like(cs.UserSubscription.NamePlan!, kw))
//                );
//            }
//            // --- Sắp xếp ưu tiên ---
//            // Ưu tiên: Active lên đầu → sắp xếp theo ngày ExpiredAt gần nhất → sau đó CreatedAt giảm dần
//            q = q.OrderByDescending(cs => cs.Status == SubscriptionStatus.Active)
//                 .ThenBy(cs => cs.ExpiredAt)
//                 .ThenByDescending(cs => cs.CreatedAt);

//            // Nếu có sortColumn được gửi từ request → áp dụng bổ sung
//            if (!string.IsNullOrWhiteSpace(request.SortColumn)
//                && CompanySubscriptionPagedRequest.SortMap.TryGetValue(request.SortColumn, out var mapped))
//            {
//                q = request.SortDescending
//                    ? q.OrderByDescending(e => EF.Property<object>(e, mapped))
//                    : q.OrderBy(e => EF.Property<object>(e, mapped));
//            }

//            // --- Phân trang ---
//            return await q.ToPagedResultAsync(request, ct);
//        }
//        public async Task<List<CompanySubscription>> GetAllActiveByCompanyIdAsync(Guid companyId, CancellationToken ct = default)
//        {
//            var now = DateTime.UtcNow;

//            var activeSubscriptions = await _context.CompanySubscriptions
//                .AsNoTracking()
//                .Include(cs => cs.CompanySubscriptionEntitlements)
//                .Where(cs =>
//                    cs.CompanyId == companyId &&
//                    cs.Status == SubscriptionStatus.Active &&
//                    (cs.ExpiredAt == null || cs.ExpiredAt > now))
//                .OrderBy(cs => cs.ExpiredAt)
//                .ThenByDescending(cs => cs.CreatedAt)
//                .ToListAsync(ct);

//            return activeSubscriptions;
//        }
//        public Task<CompanySubscription?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default)
//            => _context.CompanySubscriptions
//                .Include(cs => cs.Company)
//                .Include(cs => cs.UserSubscription)
//                .Include(cs => cs.CompanySubscriptionEntitlements)
//                .FirstOrDefaultAsync(cs => cs.Id == id, ct);

//        public async Task UseFeatureAsync(Guid companySubscriptionId, FeatureKeys featureKey, int quantity, CancellationToken ct = default)
//        {
//            if (quantity <= 0)
//                throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

//            var now = DateTime.UtcNow;

//            // 1. Lấy gói company subscription kèm entitlements
//            var companySub = await _context.CompanySubscriptions
//                .Include(cs => cs.CompanySubscriptionEntitlements)
//                .FirstOrDefaultAsync(cs => cs.Id == companySubscriptionId, ct);

//            if (companySub == null)
//                throw CustomExceptionFactory.CreateNotFoundError("Company subscription not found");

//            if (companySub.Status != SubscriptionStatus.Active || (companySub.ExpiredAt != null && companySub.ExpiredAt <= now))
//                throw CustomExceptionFactory.CreateBadRequestError("Company subscription is inactive or expired");

//            // 2. Lấy entitlement tương ứng
//            var entitlement = companySub.CompanySubscriptionEntitlements.FirstOrDefault(e => e.FeatureKey == featureKey);
//            if (entitlement == null)
//                throw CustomExceptionFactory.CreateBadRequestError($"Feature {featureKey} not available in this subscription");

//            if (entitlement.Remaining < quantity)
//                throw CustomExceptionFactory.CreateBadRequestError($"Not enough remaining quota for feature {featureKey}");

//            // 3. Trừ remaining
//            entitlement.Remaining -= quantity;

//            // 4. Lưu lại
//            _context.CompanySubscriptionEntitlements.Update(entitlement);
//            await _context.SaveChangesAsync(ct);
//        }
//    }
//}
