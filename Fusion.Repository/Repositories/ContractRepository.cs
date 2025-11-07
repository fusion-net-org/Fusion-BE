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
    public class ContractRepository: GenericRepository<Contract>, IContractRepository
    {
        private readonly FusionDbContext _context;

        public ContractRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Contract> CreateContractAsync(Guid userId, Guid projectRequestId, Contract contract, CancellationToken ct = default)
        {
            var projectRequest = await _context.ProjectRequests.FirstOrDefaultAsync(x => x.Id == projectRequestId, ct);

            if (projectRequest == null)
                throw CustomExceptionFactory.CreateNotFoundError("Project request not found.");

            // Kiểm tra user có phải requester hay không
            if (projectRequest.CreatedBy != userId)
                throw CustomExceptionFactory.CreateBadRequestError("You are not the requester of this project.");

            bool hasContract = await _context.Contracts.AnyAsync(c => c.ProjectRequestId == projectRequestId, ct);

            if (hasContract)
                throw CustomExceptionFactory.CreateBadRequestError("This project request already has a contract.");

            contract.ProjectRequestId = projectRequestId;

            await _context.Contracts.AddAsync(contract);
            await _context.SaveChangesAsync(ct);

            return contract;
        }

        public async Task<List<Contract>> GetAllContractsAsync(CancellationToken ct = default)
        {
            return await _context.Contracts
                .Include(x => x.ContractAppendices)
                .OrderByDescending(x => x.EffectiveDate)
                .ToListAsync(ct);
        }

        public async Task<Contract?> GetContractByIdAsync(Guid contractId, CancellationToken ct = default)
        {
            return await _context.Contracts
                .Include(x => x.ContractAppendices)
                .FirstOrDefaultAsync(x => x.Id == contractId, ct);
        }

        public async Task<Contract?> UpdateContractAsync(Guid contractId, Guid userId, Contract request, List<string> appendices ,CancellationToken ct = default)
        {
            var contract = await _context.Contracts.Include(x => x.ContractAppendices).FirstOrDefaultAsync(x => x.Id == contractId, ct);
            if (contract == null)
                throw CustomExceptionFactory.CreateNotFoundError("Contract not found.");

            var projectRequest = await _context.ProjectRequests.FirstOrDefaultAsync(x => x.Id == contract.ProjectRequestId, ct);
            if (projectRequest == null)
                throw CustomExceptionFactory.CreateNotFoundError("Project request not found.");

            if (projectRequest.CreatedBy != userId)
                throw CustomExceptionFactory.CreateBadRequestError("You are not the requester of this project.");

            contract.ContractCode = request.ContractCode ?? contract.ContractCode;
            contract.ContractName = request.ContractName ?? contract.ContractName;
            contract.Budget = request.Budget ?? contract.Budget;
            contract.EffectiveDate = request.EffectiveDate ?? contract.EffectiveDate;
            contract.ExpiredDate = request.ExpiredDate ?? contract.ExpiredDate;

            _context.ContractAppendices.RemoveRange(contract.ContractAppendices);

            contract.ContractAppendices.Clear();

            var updatedAppendices = new List<ContractAppendix>();


            int index = 1;

            foreach (var item in appendices)
            {
                updatedAppendices.Add(
                    new ContractAppendix
                    {
                        Id = Guid.NewGuid(),
                        ContractId = contract.Id,
                        AppendixCode = $"PL-{index:00}",
                        Title = item,
                        Description = null,
                        FilePath = null,
                        CreatedAt = DateTime.UtcNow
                    }
                );

                index++;
            }

            await _context.ContractAppendices.AddRangeAsync(updatedAppendices);
            await _context.SaveChangesAsync(ct);

            return contract;
        }
    }
}
