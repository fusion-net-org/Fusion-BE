using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.IRepositories
{
    public interface IContractAppendixRepository: IGenericRepository<ContractAppendix>
    {
        Task<List<ContractAppendix>> CreateContractAppendixAsync(Guid contractId, List<string> appendices, CancellationToken ct = default);
    }
}
