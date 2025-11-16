

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Fusion.Repository.Repositories;

public class TransactionPaymentRepository : GenericRepository<TransactionPayment>, ITransactionPaymentRepository
{
    private readonly FusionDbContext _context;
    public TransactionPaymentRepository(FusionDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<TransactionPayment?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default)
      => _context.TransactionPayments
                  .AsNoTracking()
                  .Include(x => x.User)
                  .Include(x => x.SubscriptionPlan).ThenInclude(p => p.Price)
                  .Include(x => x.SubscriptionPlan).ThenInclude(p => p.Features)
                  .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<TransactionPayment> CreateAsync(TransactionPayment entity, CancellationToken ct = default)
    {
        _context.TransactionPayments.Add(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> UpdateAsync(TransactionPayment entity, CancellationToken ct = default)
    {
        var exist = await _context.TransactionPayments
                                  .FirstOrDefaultAsync(x => x.Id == entity.Id, ct);
        if (exist == null) return false;

        exist.Amount = entity.Amount;
        exist.Description = entity.Description;
        exist.AccountNumber = entity.AccountNumber;
        exist.Reference = entity.Reference;
        exist.TransactionDateTime = entity.TransactionDateTime;
        exist.Currency = entity.Currency;
        exist.CounterAccountBankId = entity.CounterAccountBankId;
        exist.CounterAccountBankName = entity.CounterAccountBankName;
        exist.CounterAccountName = entity.CounterAccountName;
        exist.CounterAccountNumber = entity.CounterAccountNumber;
        exist.PaymentMethod = entity.PaymentMethod;
        exist.Status = entity.Status;
        exist.OrderCode = entity.OrderCode;
        exist.PaymentLinkId = entity.PaymentLinkId;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var exist = await _context.TransactionPayments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (exist == null) return false;

        _context.TransactionPayments.Remove(exist);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public Task<bool> ExistsOrderCodeAsync(long orderCode, CancellationToken ct = default)
        => _context.TransactionPayments.AnyAsync(x => x.OrderCode == orderCode, ct);

    public Task<bool> ExistsPaymentLinkIdAsync(string paymentLinkId, CancellationToken ct = default)
      => _context.TransactionPayments.AnyAsync(x => x.PaymentLinkId == paymentLinkId, ct);


    public Task<TransactionPayment?> GetByOrderCodeAsync(long orderCode, CancellationToken ct = default)
          => _context.TransactionPayments.AsNoTracking()
                         .FirstOrDefaultAsync(x => x.OrderCode == orderCode, ct);

    public Task<TransactionPayment?> GetByPaymentLinkIdAsync(string paymentLinkId, CancellationToken ct = default)
       => _context.TransactionPayments.AsNoTracking()
                   .FirstOrDefaultAsync(x => x.PaymentLinkId == paymentLinkId, ct);

    public async Task<PagedResult<TransactionPayment>> GetPagedAsync(TransactionPaymentPagedRequest request, CancellationToken ct = default)
    {
        var q = _context.TransactionPayments
                        .AsNoTracking()
                        .Include(tp => tp.User)
                        .Include(tp => tp.SubscriptionPlan)
                        .AsQueryable();

        // ----- Filters -----
        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            var pattern = $"%{request.UserName.Trim()}%";
            q = q.Where(tp => tp.User.UserName != null &&
                              EF.Functions.Like(tp.User.UserName, pattern));
        }

        if (!string.IsNullOrWhiteSpace(request.PlanName))
        {
            var pattern = $"%{request.PlanName.Trim()}%";
            q = q.Where(tp => tp.SubscriptionPlan.Name != null &&
                              EF.Functions.Like(tp.SubscriptionPlan.Name, pattern));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            // Nếu bạn lưu Status là enum string (PaymentStatus.*.ToString())
            q = q.Where(tp => tp.Status == request.Status);
        }

        if (request.TransactionAt.From.HasValue)
            q = q.Where(tp => tp.TransactionDateTime >= request.TransactionAt.From.Value);

        if (request.TransactionAt.To.HasValue)
            q = q.Where(tp => tp.TransactionDateTime <= request.TransactionAt.To.Value);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim();
            var pattern = $"%{kw}%";
            q = q.Where(tp =>
                (tp.Reference != null && EF.Functions.Like(tp.Reference, pattern)) ||
                (tp.Description != null && EF.Functions.Like(tp.Description, pattern)) ||
                (tp.PaymentLinkId != null && EF.Functions.Like(tp.PaymentLinkId, pattern)) ||
                (tp.Currency != null && EF.Functions.Like(tp.Currency, pattern)) ||
                // OrderCode là long? → convert sang chuỗi để tìm kiếm
                (tp.OrderCode.HasValue && EF.Functions.Like(tp.OrderCode.Value.ToString(), $"%{kw}%"))
            );
        }

        // ----- Default sort (nếu client không truyền) -----
        if (string.IsNullOrWhiteSpace(request.SortColumn))
        {
            request.SortColumn = nameof(TransactionPayment.TransactionDateTime);
            request.SortDescending = true;
        }

        return await q.ToPagedResultAsync(request, ct);
    }

    public async Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default)
    {
        return await _context.TransactionPayments
            .Where(t => t.Status == "Success")
            .SumAsync(t => (decimal?)t.Amount, ct) ?? 0;
    }

    public async Task<IEnumerable<MonthlyStats>> GetMonthlyStatsAsync(int year, CancellationToken ct = default)
    {
        var monthlyRevenue = await _context.TransactionPayments
            .Where(t => t.TransactionDateTime.HasValue &&
                        t.TransactionDateTime.Value.Year == year &&
                        t.Status == "Success")
            .GroupBy(t => t.TransactionDateTime.Value.Month)
            .Select(g => new { Month = g.Key, Revenue = g.Sum(x => x.Amount) })
            .ToListAsync();

        var monthlyUsers = await _context.Users
            .Where(u => u.CreateAt.Year == year)
            .GroupBy(u => u.CreateAt.Month)
            .Select(g => new { Month = g.Key, Users = g.Count() })
            .ToListAsync();

        var monthlyCompanies = await _context.Companies
            .Where(c => c.CreateAt.Year == year)
            .GroupBy(c => c.CreateAt.Month)
            .Select(g => new { Month = g.Key, Companies = g.Count() })
            .ToListAsync();

        var months = Enumerable.Range(1, 12).Select(m => new MonthlyStats
        {
            Month = m,
            Revenue = monthlyRevenue.FirstOrDefault(x => x.Month == m)?.Revenue ?? 0,
            Users = monthlyUsers.FirstOrDefault(x => x.Month == m)?.Users ?? 0,
            Companies = monthlyCompanies.FirstOrDefault(x => x.Month == m)?.Companies ?? 0
        });

        return months;
    }

    public async Task<IEnumerable<PlanRate>> GetTopPlanRateAsync(CancellationToken token = default)
    {
        var successPayments = _context.TransactionPayments
            .Where(t => t.Status == "Success");

        var totalCount = await successPayments.CountAsync(token);

        var planGroups = await successPayments
            .GroupBy(t => t.PlanId)
            .Select(g => new
            {
                PlanId = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        var planNames = await _context.SubscriptionPlans
            .ToDictionaryAsync(p => p.Id, p => p.Name);

        var list = planGroups.Select(p => new
        {
            PlanName = planNames.ContainsKey(p.PlanId) ? planNames[p.PlanId] : "Unknown",
            p.Count
        })
        .OrderByDescending(x => x.Count)
        .ToList();

        var top3 = list.Take(3).ToList();
        var otherCount = list.Skip(3).Sum(x => x.Count);

        var result = top3.Select(x => new PlanRate
        {
            PlanName = x.PlanName,
            Percentage = totalCount == 0 ? 0 : Math.Round((decimal)x.Count / totalCount * 100, 2)
        }).ToList();

        if (otherCount > 0)
        {
            result.Add(new PlanRate
            {
                PlanName = "Other",
                Percentage = Math.Round((decimal)otherCount / totalCount * 100, 2)
            });
        }

        return result;
    }


}

