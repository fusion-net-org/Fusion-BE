using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page.Contract;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class ContractAppendixRepository: GenericRepository<ContractAppendix>, IContractAppendixRepository
    {
        private readonly FusionDbContext _context;

        public ContractAppendixRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<ContractAppendix>> CreateContractAppendixAsync(Guid contractId, List<CreateAppendixRequest> appendices , CancellationToken ct = default)
        {
            var contract = await _context.Contracts
                .Include(x => x.ContractAppendices)
                .FirstOrDefaultAsync(x => x.Id == contractId, ct);
            if (contract == null)
                throw CustomExceptionFactory.CreateNotFoundError("Contract not found.");

            //if (appendices == null || !appendices.Any())
            //    throw CustomExceptionFactory.CreateBadRequestError("Appendix list is empty.");

            var createdAppendices = new List<ContractAppendix>();

            int index = contract.ContractAppendices.Count + 1;

            foreach (var item in appendices)
            {
                var appendix = new ContractAppendix
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    AppendixCode = $"PL-{index:00}",
                    Title = item.Title,
                    Description = item.Description,
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
        public async Task<List<ContractAppendix>> UpdateContractAppendixAsync(
             Guid contractId,
             List<UpdateAppendixRequest> appendices,
             CancellationToken ct = default)
        {
            var contract = await _context.Contracts
                .Include(c => c.ContractAppendices)
                .FirstOrDefaultAsync(c => c.Id == contractId, ct);

            if (contract == null)
                throw CustomExceptionFactory.CreateNotFoundError("Contract not found.");

            var existing = contract.ContractAppendices.ToList();

            var requestWithIds = appendices.Select(a => new
            {
                Id = a.Id, 
                Title = a.Title,
                Description = a.Description
            }).ToList();

            int index = 1;

            foreach (var item in requestWithIds)
            {
                ContractAppendix appendix = null;

                if (item.Id != Guid.Empty)
                {
                    appendix = existing.FirstOrDefault(e => e.Id == item.Id);
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
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ContractAppendices.Add(appendix);
                }

                index++;
            }

            await _context.SaveChangesAsync(ct);

            return await _context.ContractAppendices
                .Where(a => a.ContractId == contractId)
                .OrderBy(a => a.AppendixCode)
                .ToListAsync(ct);
        }


    }
}
