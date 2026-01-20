using Fusion.Repository.Bases.Page;
using Fusion.Repository.IRepositories;
using Fusion.Service.ViewModels.Tickets.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.IServices
{
    public interface ITicketHistoryService
    {
        Task<List<TicketHistoryResponse>> GetByTicketIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default);
        Task<PagedResult<TicketHistoryResponse>> GetTicketHistoryByTicketIdAsync(TicketHistoryPagedRequest request,CancellationToken cancellationToken = default);
    }

}
