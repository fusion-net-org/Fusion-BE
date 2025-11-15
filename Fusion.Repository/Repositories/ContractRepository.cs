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
        public async Task<bool> ContractExistsAsync(Guid contractId, CancellationToken ct)
        {
            return await _context.Contracts.AnyAsync(c => c.Id == contractId, ct);
        }

        public async Task<Contract> CreateContractAsync(Guid userId,  Contract contract, CancellationToken ct = default)
        {
            contract.CreatedBy = userId;
            contract.CreateAt = DateTime.UtcNow;

            await _context.Contracts.AddAsync(contract);
            await _context.SaveChangesAsync(ct);

            return contract;
        }
        public async Task<Contract> ApproveContractAsync(Guid contractId, Guid userId, CancellationToken ct = default)
        {
            var contract = await _context.Contracts
                .Include(x => x.ProjectRequest)
                .FirstOrDefaultAsync(x => x.Id == contractId, ct);

            if (contract == null)
                throw CustomExceptionFactory.CreateNotFoundError("Contract not found.");

            var project = contract.ProjectRequest;
            if (project.ExecutorCompanyId != userId)
                throw CustomExceptionFactory.CreateBadRequestError("You cannot approve this contract.");

            if (contract.Status != "PENDING")
                throw CustomExceptionFactory.CreateBadRequestError("Contract cannot be approved in current state.");

            contract.Status = "APPROVED";
            await _context.SaveChangesAsync(ct);

            return contract;
        }
        public async Task<Contract> RejectContractAsync(Guid contractId, Guid userId, string reason, CancellationToken ct = default)
        {
            var contract = await _context.Contracts
                .Include(x => x.ProjectRequest)
                .FirstOrDefaultAsync(x => x.Id == contractId, ct);

            if (contract == null)
                throw CustomExceptionFactory.CreateNotFoundError("Contract not found.");

            var project = contract.ProjectRequest;
            if (project.ExecutorCompanyId != userId)
                throw CustomExceptionFactory.CreateBadRequestError("You cannot reject this contract.");

            if (contract.Status != "PENDING")
                throw CustomExceptionFactory.CreateBadRequestError("Contract cannot be rejected in current state.");

            contract.Status = "REJECTED";
            project.ReasonReject = reason;

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


            contract.ContractCode = request.ContractCode ?? contract.ContractCode;
            contract.ContractName = request.ContractName ?? contract.ContractName;
            contract.Budget = request.Budget ?? contract.Budget;
            contract.EffectiveDate = request.EffectiveDate ?? contract.EffectiveDate;
            contract.ExpiredDate = request.ExpiredDate ?? contract.ExpiredDate;

            contract.UpdatedBy = userId;
            contract.UpdateAt = DateTime.UtcNow;

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
        public async Task<Contract> UpdateContractStatusAsync(Guid contractId, Guid userId, string status, CancellationToken ct = default)
        {
            var contract = await _context.Contracts
                .Include(x => x.ContractAppendices)
                .FirstOrDefaultAsync(x => x.Id == contractId, ct);

            if (contract == null)
                throw CustomExceptionFactory.CreateNotFoundError("Contract not found.");

            contract.Status = status;
            contract.UpdatedBy = userId;
            contract.UpdateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return contract;
        }
        public async Task<Contract> UpdateContractAttachmentAsync(Guid contractId, string attachmentUrl, Guid userId, CancellationToken ct = default)
        {
            var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Id == contractId, ct);
            if (contract == null)
                throw CustomExceptionFactory.CreateNotFoundError("Contract not found.");

            contract.Attachment = attachmentUrl;
            contract.UpdatedBy = userId;
            contract.UpdateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return contract;
        }

    }
}
