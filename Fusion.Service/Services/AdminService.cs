
using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Admin.Responses;
using Fusion.Service.ViewModels.TransactionPayment.Responses.Overview;

namespace Fusion.Service.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ITransactionPaymentRepository _transactionPaymentRepository;
    public AdminService(IProjectRepository projectRepository, ICompanyRepository companyRepository, ITransactionPaymentRepository transactionPaymentRepository,
        IUserRepository userRepository)
    {
        _projectRepository = projectRepository;
        _companyRepository = companyRepository;
        _transactionPaymentRepository = transactionPaymentRepository;
        _userRepository = userRepository;
    }

    public async Task<OverviewDashBoardResponse> GetTotalsAsync(CancellationToken ct = default)
    {
        var totalCompanies = await _companyRepository.GetTotalCompaniesAsync(ct);
        var totalProjects = await _projectRepository.GetTotalProjectsAsync(ct);
        var totalUsers = await _userRepository.GetTotalUsersAsync(ct);
        var totalRevenue = await _transactionPaymentRepository.GetTotalRevenueAsync(ct);

        return new OverviewDashBoardResponse
        {
            CompanyCount = totalCompanies,
            ProjectCount = totalProjects,
            UserCount = totalUsers,
            RevenueSum = totalRevenue
        };
    }
        public async Task<IReadOnlyList<PlanPurchaseRatioItemResponse>> GetPlanPurchaseRatioAsync(CancellationToken ct = default)
    {
        var rows = await _transactionPaymentRepository.GetPlanPurchaseCountsAsync(ct);

        if (rows == null || rows.Count == 0)
            return Array.Empty<PlanPurchaseRatioItemResponse>();

        var ordered = rows
            .OrderByDescending(x => x.TransactionsCount)
            .ToList();

        var total = ordered.Sum(x => (long)x.TransactionsCount);
        if (total == 0)
            return Array.Empty<PlanPurchaseRatioItemResponse>();

        var top = ordered.Take(3).ToList();
        var others = ordered.Skip(3).ToList();

        var result = new List<PlanPurchaseRatioItemResponse>();
        decimal sumTopPercent = 0;

        foreach (var row in top)
        {
            var percent = Math.Round(
                row.TransactionsCount * 100m / total,
                2,
                MidpointRounding.AwayFromZero);

            sumTopPercent += percent;

            result.Add(new PlanPurchaseRatioItemResponse
            {
                SubscriptionPlanId = row.SubscriptionPlanId,
                PlanName = string.IsNullOrWhiteSpace(row.PlanName)
                    ? "Unnamed plan"
                    : row.PlanName,
                TransactionsCount = row.TransactionsCount,
                Percentage = percent,
                IsOther = false
            });
        }

        if (others.Count > 0)
        {
            var otherCount = others.Sum(x => x.TransactionsCount);
            var otherPercent = Math.Round(
                otherCount * 100m / total,
                2,
                MidpointRounding.AwayFromZero);

            result.Add(new PlanPurchaseRatioItemResponse
            {
                SubscriptionPlanId = Guid.Empty,   // Other group
                PlanName = "Other",
                TransactionsCount = otherCount,
                Percentage = otherPercent,
                IsOther = true
            });
        }

        return result;
    }
    public async Task<PlatformYearOverviewResponse> GetPlatformYearOverviewAsync(int year,CancellationToken ct = default)
    {
        if (year <= 0)
        {
            year = DateTime.UtcNow.AddHours(7).Year;
        }

        var userPoints = await _userRepository.GetMonthlyNewUsersInYearAsync(year, ct);
        var companyPoints = await _companyRepository.GetMonthlyNewCompaniesInYearAsync(year, ct);
        var tranPoints = await _transactionPaymentRepository.GetMonthlyAmountInYearAsync(year, ct);

        var userDict = userPoints.ToDictionary(x => x.Month, x => x.NewUsers);
        var companyDict = companyPoints.ToDictionary(x => x.Month, x => x.NewCompanies);
        var tranDict = tranPoints.ToDictionary(x => x.Month, x => x.TotalAmount);


        var overview = new PlatformYearOverviewResponse
        {
            Year = year,
            Months = Enumerable.Range(1, 12)
                .Select(m => new PlatformMonthlyPointResponse
                {
                    Month = m,
                    NewUsers = userDict.TryGetValue(m, out var u) ? u : 0,
                    NewCompanies = companyDict.TryGetValue(m, out var c) ? c : 0,
                    TotalTransactionAmount = tranDict.TryGetValue(m, out var a) ? a : 0m
                })
                .ToList()
        };

        overview.TotalNewUsers = overview.Months.Sum(x => x.NewUsers);
        overview.TotalNewCompanies = overview.Months.Sum(x => x.NewCompanies);
        overview.TotalTransactionAmount = overview.Months.Sum(x => x.TotalTransactionAmount);

        return overview;
    }
}
