using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories;

public class CompanySubscriptionEntryRepository : GenericRepository<CompanySubscriptionEntry> , ICompanySubscriptionEntryRepository
{
    private readonly FusionDbContext _ctx;
    public CompanySubscriptionEntryRepository(FusionDbContext context) : base(context)
    {
        _ctx = context;
    }

    public async Task<CompanySubscriptionEntry> CreateAsync(Guid companySubscriptionId, Guid actorUserId,Guid companyId, CancellationToken ct = default)
    {
            var sub = await _ctx.CompanySubscriptions
                    .FirstOrDefaultAsync(s => s.Id == companySubscriptionId, ct);
            if (sub == null)
                throw CustomExceptionFactory.CreateNotFoundError("Company subscription.");

            var user = await _ctx.CompanyMembers
                .FirstOrDefaultAsync(m => m.UserId == actorUserId && m.CompanyId == companyId, ct);

            if (user == null)
            throw CustomExceptionFactory.CreateNotFoundError("User");

        if (sub.Status != SubscriptionStatus.Active)
            throw CustomExceptionFactory.CreateBadRequestError(
                "Company subscription is not active.");

        if (sub.ExpiredAt.HasValue && sub.ExpiredAt.Value < DateTimeOffset.UtcNow)
            throw CustomExceptionFactory.CreateBadRequestError(
                "Company subscription has expired.");

        // 2. Nếu member đã có entry -> trả về luôn, không đếm lại
        var existing = await _ctx.CompanySubscriptionEntries
            .FirstOrDefaultAsync(e =>
                e.CompanySubscriptionId == companySubscriptionId &&
                e.CompanyMemberId == user.Id,
                ct);

        if (existing != null)
        {
            return existing;
        }
            // 3. Check seat limit
            if (sub.SeatsLimitSnapshot.HasValue)
            {
                var used = sub.SeatsLimitUnit ?? 0;
                if (used >= sub.SeatsLimitSnapshot.Value)
                    throw CustomExceptionFactory.CreateBadRequestError(
                        "No remaining seats for this company subscription.");
            }

            // 4. Tạo entry mới
            var entry = new CompanySubscriptionEntry
            {
                CompanySubscriptionId = companySubscriptionId,
                CompanyMemberId = user.Id,
                UsedAt = DateTimeOffset.UtcNow
            };

            await _ctx.CompanySubscriptionEntries.AddAsync(entry, ct);

            // 5. Tăng SeatsLimitUnit nếu có limit
            if (sub.SeatsLimitSnapshot.HasValue)
            {
                sub.SeatsLimitUnit = (sub.SeatsLimitUnit ?? 0) + 1;
                sub.UpdatedAt = DateTimeOffset.UtcNow;
                _ctx.CompanySubscriptions.Update(sub);
            }

            return entry;
    }
    public async Task<List<CompanySubscriptionEntry>> GetByCompanySubscriptionIdAsync(Guid companySubscriptionId, CancellationToken ct = default)
    {
        return await _ctx.CompanySubscriptionEntries
                   .AsNoTracking()
                   .Include(e => e.CompanyMember)
                       .ThenInclude(m => m.User) 
                   .Where(e => e.CompanySubscriptionId == companySubscriptionId)
                   .ToListAsync(ct);
    }
}
