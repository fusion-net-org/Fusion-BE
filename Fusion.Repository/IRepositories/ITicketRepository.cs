using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Ticket;
using Fusion.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.IRepositories
{
	public interface ITicketRepository
	{
		Task<PagedResult<Ticket>> GetPageTicketshAsync(TicketPagedSearchRequest request, CancellationToken cancellationToken = default);
		Task<Ticket?> GetTicketByIdAsync(Guid Id);
		Task<Ticket?> GetTicketByTicketName(string ticketName);
		Task<Ticket?> AddTicketAsync(Ticket newTicket, CancellationToken cancellationToken = default);
		Task<Ticket?> UpdateTicketAsync(Guid ticketId, Ticket updateTicket, CancellationToken cancellationToken = default);
		Task<bool?> DeleteTicketAsync(Ticket ticket, CancellationToken cancellationToken = default);

	}
}
