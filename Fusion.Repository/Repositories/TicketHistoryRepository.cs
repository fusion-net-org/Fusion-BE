using Fusion.Repository.Bases.Page;
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
    public class TicketHistoryRepository : ITicketHistoryRepository
    {
        private readonly FusionDbContext _context;

        public TicketHistoryRepository(FusionDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TicketHistory history, CancellationToken cancellationToken = default)
        {
            history.CreatedAt = DateTime.UtcNow.AddHours(7);
            await _context.TicketHistories.AddAsync(history, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<TicketHistory>> GeTTicketHistoryByTicketIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            return await _context.TicketHistories
                .Include(th => th.PerformedByUser)
                .Where(th => th.TicketId == ticketId)
                .OrderByDescending(th => th.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResult<TicketHistory>> GetTicketHistoryByTicketIdAsync(TicketHistoryPagedRequest request,CancellationToken cancellationToken = default)
        {
            var query = _context.TicketHistories
                .Include(th => th.PerformedByUser)
                .Where(th => th.TicketId == request.TicketId)
                .OrderByDescending(th => th.CreatedAt)
                .AsQueryable();

            return await query.ToPagedResultAsync(request, cancellationToken);
        }

    }

}
