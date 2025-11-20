

using Fusion.Repository.IRepositories;
using Fusion.Repository.ViewModels;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Admin.Responses;

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

    public Task<OverviewDashBoardResponse> OverviewDashBoard(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<OverviewDashBoardResponse> GetTotalsAsync(CancellationToken ct = default)
    {
        var totalCompanies = await _companyRepository.GetTotalCompaniesAsync(ct);
        var totalProjects = await _projectRepository.GetTotalProjectsAsync(ct);
        var totalUsers = await _userRepository.GetTotalUsersAsync(ct);
        //var totalRevenue = await _transactionPaymentRepository.GetTotalRevenueAsync(ct);

        return new OverviewDashBoardResponse
        {
            CompanyCount = totalCompanies,
            ProjectCount = totalProjects,
            UserCount = totalUsers,
            //RevenueSum = totalRevenue
        };
    }

    //public async Task<IEnumerable<MonthlyStats>> GetMonthlyStatsAsync(CancellationToken cancellationToken =  default)
    //{
    //    int currentYear = DateTime.UtcNow.Year;
    //    return await _transactionPaymentRepository.GetMonthlyStatsAsync(currentYear, cancellationToken);
    //}

    //public async Task<IEnumerable<PlanRate>> GetTopPlanRateAsync(CancellationToken token = default)
    //{
    //    return await _transactionPaymentRepository.GetTopPlanRateAsync(token);
    //}


}

//    public async Task<OverviewDashBoardResponse> OverviewDashBoard(CancellationToken cancellationToken = default)
//    {
//        var userCount = await _userRepository.GetAllUserAsync();
//        var companyCount = await _companyRepository.GetAllCompanyAsync();
//        var project = await _projectRepository.GetAllProjectCountAsync();
//        var transaction = await _transactionPaymentRepository.GetTotalRevenueSuccessAsync();

//        return new OverviewDashBoardResponse
//        {
//            UserCount = userCount,
//            CompanyCount = companyCount,
//            ProjectCount = project,
//            RevenueSum = transaction,
//        };
//    }
//}
