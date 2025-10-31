

using Fusion.Repository.IRepositories;
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

    public async Task<OverviewDashBoardResponse> OverviewDashBoard(CancellationToken cancellationToken = default)
    {
        var userCount = await _userRepository.GetAllUserAsync();
        var companyCount = await _companyRepository.GetAllCompanyAsync();
        var project = await _projectRepository.GetAllProjectCountAsync();
        var transaction = await _transactionPaymentRepository.GetTotalRevenueSuccessAsync();

        return new OverviewDashBoardResponse
        {
            UserCount = userCount,
            CompanyCount = companyCount,
            ProjectCount = project,
            RevenueSum = transaction,
        };
    }
}
