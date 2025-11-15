using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Page.Contract;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface IContractAppendixRepository: IGenericRepository<ContractAppendix>
    {
        Task<List<ContractAppendix>> CreateContractAppendixAsync(Guid contractId, List<CreateAppendixRequest> appendices, CancellationToken ct = default);
    }
}
