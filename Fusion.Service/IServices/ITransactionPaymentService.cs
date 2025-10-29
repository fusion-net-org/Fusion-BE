

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Service.ViewModels.TransactionPayment.Requests;
using Fusion.Service.ViewModels.TransactionPayment.Responses;

namespace Fusion.Service.IServices;

public interface ITransactionPaymentService
{
    //Task<TransactionPayment> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TransactionPaymentResponse> CreateTransactionPaymentAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);
    Task<TransactionPaymentResponse> GetTransactionByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Guid> GetLasterTransactionForUserAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<TransactionForAdminResponse>> GetAllTransactionForAdminAsync(
        AdminTransactionSearch request,
        CancellationToken cancellationToken = default);
}
