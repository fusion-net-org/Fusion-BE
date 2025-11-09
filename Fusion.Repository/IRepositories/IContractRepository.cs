using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.IRepositories
{
    public interface IContractRepository: IGenericRepository<Contract> 
    {
        Task<Contract> CreateContractAsync(Guid userId, Guid projectRequestId, Contract contract, CancellationToken ct = default);

        Task<List<Contract>> GetAllContractsAsync(CancellationToken ct = default);

        Task<Contract?> GetContractByIdAsync(Guid contractId, CancellationToken ct = default);

        Task<Contract?> UpdateContractAsync(Guid contractId, Guid userId, Contract request, List<string> appendices, CancellationToken ct = default);
    }
}
