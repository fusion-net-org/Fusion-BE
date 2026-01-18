using Fusion.Repository.Bases.Page;
using Fusion.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.IRepositories
{
    public interface ITicketHistoryRepository
    {
        Task AddAsync(TicketHistory history, CancellationToken cancellationToken = default);
        Task<List<TicketHistory>> GeTTicketHistoryByTicketIdAsync(Guid ticketId,CancellationToken cancellationToken = default);

        Task<PagedResult<TicketHistory>> GetTicketHistoryByTicketIdAsync(TicketHistoryPagedRequest request,CancellationToken cancellationToken = default);
    }
    public class TicketHistoryPagedRequest : PagedRequest
    {
        public Guid TicketId { get; set; }
    }

}
