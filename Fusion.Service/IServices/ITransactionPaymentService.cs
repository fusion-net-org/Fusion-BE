

using Fusion.Service.ViewModels.TransactionPayment.Requests;
using Fusion.Service.ViewModels.TransactionPayment.Responses;

namespace Fusion.Service.IServices;

public interface ITransactionPaymentService
{
    //Task<TransactionPayment> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TransactionPaymentResponse> CreateTransactionPaymentAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);
    Task UpdateTransactionAsync(Guid id);
    Task<TransactionPaymentResponse> GetTransactionByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Guid> GetLasterTransactionForUserAsync(CancellationToken cancellationToken = default);
}
