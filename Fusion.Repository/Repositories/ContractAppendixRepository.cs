using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Repositories
{
    public class ContractAppendixRepository: GenericRepository<ContractAppendix>, IContractAppendixRepository
    {
        private readonly FusionDbContext _context;

        public ContractAppendixRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<ContractAppendix>> CreateContractAppendixAsync(Guid contractId, List<string> appendices , CancellationToken ct = default)
        {
            var contract = await _context.Contracts
                .Include(x => x.ContractAppendices)
                .FirstOrDefaultAsync(x => x.Id == contractId, ct);
            if (contract == null)
                throw CustomExceptionFactory.CreateNotFoundError("Contract not found.");

            if (appendices == null || !appendices.Any())
                throw CustomExceptionFactory.CreateBadRequestError("Appendix list is empty.");

            var createdAppendices = new List<ContractAppendix>();

            int index = contract.ContractAppendices.Count + 1;

            foreach (var item in appendices)
            {
                var appendix = new ContractAppendix
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    AppendixCode = $"PL-{index:00}",
                    Title = item,
                    Description = null,
                    FilePath = null,
                    CreatedAt = DateTime.UtcNow
                };

                createdAppendices.Add(appendix);
                index++;
            }

            await _context.ContractAppendices.AddRangeAsync(createdAppendices);
            await _context.SaveChangesAsync(ct);

            return createdAppendices;

        }

    }
}
