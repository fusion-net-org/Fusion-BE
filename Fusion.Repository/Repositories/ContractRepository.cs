using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Contract;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            contract.CreateAt = DateTime.UtcNow.AddHours(7);

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

        public async Task<List<Contract>> GetAllContractsAdminAsync(CancellationToken ct = default)
        {
            return await _context.Contracts
                .Include(x => x.ContractAppendices)
                .OrderByDescending(x => x.EffectiveDate)
                .ToListAsync(ct);
        }

        public async Task<PagedResult<Contract>> GetAllContractsAdminAsync(ContractSearchRequest request, CancellationToken ct = default)
        {
            var query = _context.Contracts
                .Include(x => x.ContractAppendices)
                .Include(x => x.RequesterCompany)
                .Include(x => x.ExecutorCompany)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.KeyWord))
            {
                var kw = request.KeyWord.ToLower().Trim();
                query = query.Where(x =>
                    x.ContractName.ToLower().Contains(kw) ||
                    x.ContractCode.ToLower().Contains(kw));
            }

            if (request.BudgetRange != null)
            {
                var from = request.BudgetRange.From;
                var to = request.BudgetRange.To;

                if (from.HasValue && !to.HasValue)
                {
                    query = query.Where(x => x.Budget >= from.Value);
                }

                if (!from.HasValue && to.HasValue)
                {
                    query = query.Where(x => x.Budget <= to.Value);
                }

                if (from.HasValue && to.HasValue)
                {
                    query = query.Where(x => x.Budget >= from.Value && x.Budget <= to.Value);
                }
            }

            if (request.Status.HasValue)
            {
                var statusLower = request.Status.Value.ToString().ToLower();

                query = query.Where(x => x.Status.ToString().ToLower() == statusLower);
            }


            if (!string.IsNullOrWhiteSpace(request.CompanyName))
            {
                var name = request.CompanyName.ToLower().Trim();
                query = query.Where(x =>
                    x.RequesterCompany.Name.ToLower().Contains(name) ||
                    x.ExecutorCompany.Name.ToLower().Contains(name));
            }

            if (request.StatusDate == null && request.DateRange != null)
                throw CustomExceptionFactory.CreateBadRequestError(
                    "Please select date type (Effective, Expired, or CreateAt).");

            if (request.StatusDate != null && request.DateRange == null)
                throw CustomExceptionFactory.CreateBadRequestError(
                    "Please provide date range for selected date type.");

            if (request.StatusDate != null && request.DateRange != null)
            {
                var from = request.DateRange.From;
                var to = request.DateRange.To;

                if (from == null || to == null)
                    throw CustomExceptionFactory.CreateBadRequestError(
                        "Date range must include both 'From' and 'To'.");

                switch (request.StatusDate)
                {
                    case ContractDateEnum.EffectiveDate:
                        query = query.Where(x =>
                            x.EffectiveDate >= from &&
                            x.EffectiveDate <= to);
                        break;

                    case ContractDateEnum.ExpiredDate:
                        query = query.Where(x =>
                            x.ExpiredDate >= from &&
                            x.ExpiredDate <= to);
                        break;

                    case ContractDateEnum.CreateAt:
                        query = query.Where(x =>
                            DateOnly.FromDateTime(x.CreateAt) >= from &&
                            DateOnly.FromDateTime(x.CreateAt) <= to
                        );
                        break;
                }
            }

            return await query.ToPagedResultAsync(request, ct);

        }

        public async Task<Contract?> GetContractByIdAsync(Guid contractId, CancellationToken ct = default)
        {
            return await _context.Contracts
                .Include(x => x.ContractAppendices.OrderBy(a => a.CreatedAt))
                .FirstOrDefaultAsync(x => x.Id == contractId, ct);
        }

        public async Task<Contract> UpdateContractAsync(
       Guid contractId,
       Guid userId,
       Contract contractToUpdate,
       List<UpdateAppendixRequest>? appendices,
       CancellationToken ct)
        {
            var contract = await _context.Contracts
                .Include(c => c.ContractAppendices)
                .FirstOrDefaultAsync(c => c.Id == contractId, ct);

            if (contract == null)
                throw CustomExceptionFactory.CreateNotFoundError("Contract not found");

            contract.ContractCode = contractToUpdate.ContractCode;
            contract.ContractName = contractToUpdate.ContractName;
            contract.EffectiveDate = contractToUpdate.EffectiveDate;
            contract.ExpiredDate = contractToUpdate.ExpiredDate;
            contract.Budget = contractToUpdate.Budget;
            contract.UpdatedBy = userId;
            contract.UpdateAt = DateTime.UtcNow.AddHours(7);

            if (appendices != null && appendices.Any())
            {
                var existing = contract.ContractAppendices.ToList();
                int index = 1;

                foreach (var item in appendices)
                {
                    ContractAppendix appendix = null;

                    if (item.Id.HasValue && item.Id.Value != Guid.Empty)
                    {
                        appendix = existing.FirstOrDefault(e => e.Id == item.Id.Value);
                    }
                    else
                    {
                        appendix = existing.FirstOrDefault(e => e.Title == item.Title);
                    }

                    if (appendix != null)
                    {
                        appendix.Title = item.Title;
                        appendix.Description = item.Description;
                        appendix.AppendixCode = $"PL-{index:00}";
                    }
                    else
                    {
                        appendix = new ContractAppendix
                        {
                            Id = Guid.NewGuid(),
                            ContractId = contractId,
                            Title = item.Title,
                            Description = item.Description,
                            AppendixCode = $"PL-{index:00}",
                            CreatedAt = DateTime.UtcNow.AddHours(7)
                        };
                        _context.ContractAppendices.Add(appendix);
                    }

                    index++;
                }

                var toRemove = existing
                    .Where(e => !appendices.Any(r => r.Id.HasValue ? r.Id.Value == e.Id : r.Title == e.Title))
                    .ToList();

                if (toRemove.Any())
                    _context.ContractAppendices.RemoveRange(toRemove);
            }

            await _context.SaveChangesAsync(ct);

            return await _context.Contracts
                .Include(c => c.ContractAppendices)
                .FirstOrDefaultAsync(c => c.Id == contractId, ct);
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
            contract.UpdateAt = DateTime.UtcNow.AddHours(7);

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
            contract.UpdateAt = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync(ct);
            return contract;
        }

    }
}
