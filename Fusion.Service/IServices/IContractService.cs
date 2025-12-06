using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Contract;
using Fusion.Service.ViewModels.Contract.Requests;
using Fusion.Service.ViewModels.Contract.Responses;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.IServices
{
    public interface IContractService
    {
        Task<bool> ContractExistsAsync(Guid contractId, CancellationToken ct);
        Task<string> UploadContractAttachmentAsync(Guid contractId, IFormFile file, Guid userId, CancellationToken ct = default);

        Task<ContractResponse> CreateContractAsync(Guid userId, CreateContractRequest request, CancellationToken ct = default);

        Task<ContractResponse> UpdateContractAsync(Guid id, Guid userId, UpdateContractRequest request, CancellationToken ct = default);

        Task<ContractResponse> GetContractByIdAsync(Guid contractId, CancellationToken ct = default);

        Task<List<ContractResponse>> GetAllContractsAsync(CancellationToken ct = default);

        Task<PagedResult<ContractResponse>> GetAllContractsAdminAsync(ContractSearchRequest request,
    CancellationToken ct = default);

        Task<ContractResponse> UpdateContractStatusAsync(Guid contractId, Guid userId, string status, CancellationToken ct = default);
    }
}
