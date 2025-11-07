using Fusion.Service.ViewModels.Contract.Requests;
using Fusion.Service.ViewModels.Contract.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.IServices
{
    public interface IContractService
    {
        Task<ContractResponse> CreateContractAsync(Guid userId, CreateContractRequest request, CancellationToken ct = default);

        Task<ContractResponse> UpdateContractAsync(Guid id, Guid userId, UpdateContractRequest request, CancellationToken ct = default);

        Task<ContractResponse> GetContractByIdAsync(Guid contractId, CancellationToken ct = default);

        Task<List<ContractResponse>> GetAllContractsAsync(CancellationToken ct = default);
    }
}
