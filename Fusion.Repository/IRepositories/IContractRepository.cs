using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Contract;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface IContractRepository: IGenericRepository<Contract> 
    {
        Task<bool> ContractExistsAsync(Guid contractId, CancellationToken ct);
        Task<Contract> CreateContractAsync(Guid userId, Contract contract, CancellationToken ct = default);
        Task<Contract> UpdateContractAttachmentAsync(Guid contractId, string attachmentUrl, Guid userId, CancellationToken ct = default);

        Task<List<Contract>> GetAllContractsAsync(CancellationToken ct = default);

        Task<PagedResult<Contract>> GetAllContractsAdminAsync(ContractSearchRequest request, CancellationToken ct = default);

        Task<Contract?> GetContractByIdAsync(Guid contractId, CancellationToken ct = default);

        Task<Contract?> UpdateContractAsync(Guid contractId, Guid userId, Contract request, List<UpdateAppendixRequest> appendices, CancellationToken ct = default);
        Task<Contract> UpdateContractStatusAsync(Guid contractId, Guid userId, string status, CancellationToken ct = default);
    }
}
