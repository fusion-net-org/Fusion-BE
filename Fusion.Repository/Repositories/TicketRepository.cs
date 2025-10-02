using Azure.Core;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Ticket;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Fusion.Repository.Repositories
{
	public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
	{
		private readonly FusionDbContext _context;

		public TicketRepository(FusionDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<PagedResult<Ticket>> GetPageTicketshAsync(TicketPagedSearchRequest request, CancellationToken cancellationToken = default)
		{
			var query = _dbSet
				.Include(x => x.TicketComments)
				.AsQueryable();

			//search
			if (!string.IsNullOrWhiteSpace(request.TicketName))
			{
				query = query.Where(t => (t.TicketName ?? "").Contains(request.TicketName));
			}

			return await query.ToPagedResultAsync(request, cancellationToken);
		}

		public async Task<Ticket?> GetTicketByIdAsync(Guid Id)
		{
			return await _context.Tickets.Include(x => x.TicketComments).SingleOrDefaultAsync(x => x.Id == Id);
		}

		public async Task<Ticket?> GetTicketByTicketName(string ticketName)
		{
			var ticket = await _context.Tickets
				.SingleOrDefaultAsync(x => x.TicketName == ticketName);

			return ticket;
		}

		public async Task<Ticket?> AddTicketAsync(Ticket newTicket, CancellationToken cancellationToken = default)
		{
			newTicket.IsDeleted = false;
			newTicket.CreatedAt = DateTime.UtcNow.AddHours(7);

			var ticket = await _context.Tickets.AddAsync(newTicket);
			await _context.SaveChangesAsync(cancellationToken);
			return ticket.Entity;
		}

		public async Task<Ticket?> UpdateTicketAsync(Guid ticketId, Ticket updateTicket, CancellationToken cancellationToken = default)
		{
			var existedTicket = await _context.Tickets.FindAsync(ticketId);

			existedTicket.TicketName = updateTicket.TicketName ?? existedTicket.TicketName;
			existedTicket.Priority = updateTicket.Priority ?? existedTicket?.Priority;
			existedTicket.Urgency = updateTicket.Urgency ?? existedTicket?.Urgency;
			existedTicket.Budget = updateTicket.Budget ?? existedTicket.Budget;
			existedTicket.Description = updateTicket.Description ?? existedTicket.Description;
			existedTicket.IsHighestUrgen = updateTicket.IsHighestUrgen;
			existedTicket.ResolvedAt = updateTicket.ResolvedAt ?? existedTicket.ResolvedAt;
			existedTicket.ClosedAt = updateTicket?.ClosedAt ?? existedTicket.ClosedAt;
			existedTicket.UpdatedAt = DateTime.UtcNow.AddHours(7);

			var ticket = _context.Tickets.Update(existedTicket);

			await _context.SaveChangesAsync(cancellationToken);
			return ticket.Entity;
		}

		public async Task<bool?> DeleteTicketAsync(Ticket ticket, CancellationToken cancellationToken = default)
		{
			ticket.IsDeleted = true;
			_context.Tickets.Update(ticket);
			await _context.SaveChangesAsync(cancellationToken);
			return true;
		}
	}
}
