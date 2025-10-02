using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Ticket;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Tickets.Requests;
using Fusion.Service.ViewModels.Tickets.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.IServices
{
	public interface ITicketService
	{
		Task<PagedResult<TicketResponse>> GetPageTicketshAsync(TicketPagedSearchRequest request, CancellationToken cancellationToken = default);
		Task<TicketResponse?> GetTicketByIdAsync(Guid id);
		Task<TicketResponse?> CreateTicketAsync(TicketRequest request, CancellationToken cancellationToken = default);
		Task<TicketResponse?> UpdateTicketAsync(TicketRequest request, Guid ticketId, CancellationToken cancellationToken = default);
		Task<bool?> DeleteTicketAsync(Guid ticketId, CancellationToken cancellationToken = default);
	}
}
