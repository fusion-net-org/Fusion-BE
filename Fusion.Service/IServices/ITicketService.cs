using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Ticket;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Tickets.Requests;
using Fusion.Service.ViewModels.Tickets.Responses;
using static Fusion.Service.Services.TicketService;

namespace Fusion.Service.IServices
{
	public interface ITicketService
	{
		Task<TicketPagedResponse> GetPageTicketshAsync(TicketPagedSearchRequest request, CancellationToken cancellationToken = default);

        Task<TicketResponse?> GetTicketByIdAsync(Guid id);
		Task<TicketResponse?> CreateTicketAsync(TicketRequest request, CancellationToken cancellationToken = default);
		Task<TicketResponse?> UpdateTicketAsync(TicketRequest request, Guid ticketId, CancellationToken cancellationToken = default);
		Task<bool?> DeleteTicketAsync(Guid ticketId,string reason, CancellationToken cancellationToken = default);
        Task<PagedResult<TicketResponse>> GetTicketsByProjectIdAsync(TicketByProjectPagedRequest request, CancellationToken cancellationToken = default);
        Task<TicketDashboardResponse> GetTicketDashboardAsync(Guid projectId, CancellationToken cancellationToken = default);
		Task<bool?> RestoreTicketAsync(Guid ticketId, CancellationToken cancellationToken = default);
        Task<TicketStatusCountResponse> GetTicketStatusCountAsync(Guid? projectId = null, Guid? companyRequestId = null, Guid? companyExecutorId = null, CancellationToken cancellationToken = default);
    }
}
