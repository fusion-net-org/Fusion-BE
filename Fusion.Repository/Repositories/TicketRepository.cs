using Azure.Core;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Ticket;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        private readonly FusionDbContext _context;

        public TicketRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        //public async Task<PagedResult<Ticket>> GetPageTicketshAsync(TicketPagedSearchRequest request, CancellationToken cancellationToken = default)
        //{
        //    var query = _dbSet
        //        .Include(x => x.TicketComments)
        //        .Include(x => x.Project)
        //        .AsQueryable();


        //    // Keyword
        //    if (!string.IsNullOrWhiteSpace(request.Keyword))
        //        query = query.Where(x => x.TicketName.Contains(request.Keyword)
        //            || x.Budget.Equals(request.Keyword)
        //            || x.Description.Equals(request.Keyword)
        //            || x.Priority.Equals(request.Keyword)
        //            || x.Project.Name.Equals(request.Keyword)
        //        );

        //    // Status
        //    if (request.Status.HasValue)
        //        query = query.Where(x => x.status == request.Status.Value.ToString());

        //    // Filter ProjectId
        //    if (request.ProjectId.HasValue)
        //    {
        //        query = query.Where(x => x.ProjectId == request.ProjectId.Value);
        //    }

        //    // ViewMode filter
        //    if (request.ViewMode == TicketViewMode.AsRequester)
        //    {
        //        if (!request.CompanyRequestId.HasValue)
        //        {
        //            return new PagedResult<Ticket>
        //            {
        //                Items = new List<Ticket>(),
        //                TotalCount = 0,
        //                PageNumber = request.PageNumber,
        //                PageSize = request.PageSize
        //            };
        //        }

        //        query = query.Where(x =>
        //            x.Project.CompanyRequestId == request.CompanyRequestId);

        //    }
        //    else if (request.ViewMode == TicketViewMode.AsExecutor)
        //    {
        //        if (!request.CompanyExecutorId.HasValue)
        //        {
        //            return new PagedResult<Ticket>
        //            {
        //                Items = new List<Ticket>(),
        //                TotalCount = 0,
        //                PageNumber = request.PageNumber,
        //                PageSize = request.PageSize
        //            };
        //        }

        //        query = query.Where(x =>
        //            x.Project.CompanyId == request.CompanyExecutorId);
        //    }

        //    return await query.ToPagedResultAsync(request, cancellationToken);
        //}

        public async Task<PagedResult<Ticket>> GetPageTicketshAsync(TicketPagedSearchRequest request, CancellationToken cancellationToken = default)
        {
            var query = BuildTicketQuery(request);
            return await query.ToPagedResultAsync(request, cancellationToken);
        }

        public IQueryable<Ticket> BuildTicketQuery(TicketPagedSearchRequest request)
        {
            var query = _dbSet
                .Include(x => x.TicketComments)
                .Include(x => x.Project)
                .AsQueryable();

            // Keyword
            if (!string.IsNullOrWhiteSpace(request.Keyword))
                query = query.Where(x => x.TicketName.Contains(request.Keyword)
                    || x.Budget.Equals(request.Keyword)
                    || x.Description.Equals(request.Keyword)
                    || x.Priority.Equals(request.Keyword)
                    || x.Project.Name.Contains(request.Keyword)
                );

            // Status
            if (request.Status.HasValue)
                query = query.Where(x => x.status == request.Status.Value.ToString());

            // ProjectId
            if (request.ProjectId.HasValue)
                query = query.Where(x => x.ProjectId == request.ProjectId.Value);

            // ViewMode
            if (request.ViewMode == TicketViewMode.AsRequester)
            {
                if (!request.CompanyRequestId.HasValue)
                    return Enumerable.Empty<Ticket>().AsQueryable();

                query = query.Where(x => x.Project.CompanyRequestId == request.CompanyRequestId);
            }
            else if (request.ViewMode == TicketViewMode.AsExecutor)
            {
                if (!request.CompanyExecutorId.HasValue)
                    return Enumerable.Empty<Ticket>().AsQueryable();

                query = query.Where(x => x.Project.CompanyId == request.CompanyExecutorId);
            }

            //create date
            if (request.CreatedFrom.HasValue)
                query = query.Where(x => x.CreatedAt >= request.CreatedFrom.Value);

            if (request.CreatedTo.HasValue)
                query = query.Where(x => x.CreatedAt <= request.CreatedTo.Value);

            // Deleted
            if (request.IsDeleted.HasValue)
                query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);

            return query;
        }


        public async Task<Ticket?> GetTicketByIdAsync(Guid Id)
        {
            return await _context.Tickets
                .Include(x => x.TicketComments)
                .Include(x => x.SubmittedByNavigation)
                .Include(x => x.Project)
                .Include(x => x.WorkflowStatus)
                .SingleOrDefaultAsync(x => x.Id == Id);
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
            newTicket.status = TicketStatusEnum.Pending.ToString();

            var ticket = await _context.Tickets.AddAsync(newTicket);
            await _context.SaveChangesAsync(cancellationToken);
            return ticket.Entity;
        }

        public async Task<Ticket?> UpdateTicketAsync(Guid ticketId, Ticket updateTicket, CancellationToken cancellationToken = default)
        {
            var existedTicket = await _context.Tickets.FindAsync(ticketId);

            existedTicket.TicketName = updateTicket.TicketName ?? existedTicket.TicketName;
            existedTicket.Priority = updateTicket.Priority ?? existedTicket?.Priority;
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

        public async Task<bool?> DeleteTicketAsync(Ticket ticket, string reason, CancellationToken cancellationToken = default)
        {
            ticket.IsDeleted = true;
            ticket.reason = reason;
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<PagedResult<Ticket>> GetTicketsByProjectIdAsync(
      TicketByProjectPagedRequest request,
      CancellationToken cancellationToken = default)
        {
            var query = _context.Tickets
                .Include(x => x.TicketComments)
                .Include(x => x.SubmittedByNavigation)
                .Where(t => t.ProjectId == request.ProjectId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.TicketName))
            {
                query = query.Where(t => (t.TicketName ?? "").Contains(request.TicketName));
            }

            if (!string.IsNullOrWhiteSpace(request.Priority))
            {
                query = query.Where(t => t.Priority == request.Priority);
            }

            if (request.MinBudget.HasValue)
            {
                query = query.Where(t => t.Budget >= request.MinBudget.Value);
            }

            if (request.MaxBudget.HasValue)
            {
                query = query.Where(t => t.Budget <= request.MaxBudget.Value);
            }

            if (request.ResolvedFrom.HasValue)
            {
                query = query.Where(t => t.ResolvedAt >= request.ResolvedFrom.Value);
            }

            if (request.ResolvedTo.HasValue)
            {
                query = query.Where(t => t.ResolvedAt <= request.ResolvedTo.Value);
            }

            if (request.ClosedFrom.HasValue)
            {
                query = query.Where(t => t.ClosedAt >= request.ClosedFrom.Value);
            }

            if (request.ClosedTo.HasValue)
            {
                query = query.Where(t => t.ClosedAt <= request.ClosedTo.Value);
            }

            if (request.CreateFrom.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= request.CreateFrom.Value);
            }

            if (request.CreateTo.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= request.CreateTo.Value);
            }



            return await query.ToPagedResultAsync(request, cancellationToken);
        }

        public async Task<List<Ticket>> GetTicketsForDashboardAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.Tickets
                .Include(t => t.Project)
                .Include(t => t.SubmittedByNavigation)
                .Include(t => t.WorkflowStatus)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync(cancellationToken);
        }
        public async Task<bool?> RestoreTicketAsync(Ticket ticket, CancellationToken cancellationToken = default)
        {
            ticket.IsDeleted = false;
            ticket.reason = null;

            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }


        public async Task<TicketStatusCountResponse> GetTicketStatusCountAsync(
            Guid? projectId = null,
            Guid? companyRequestId = null,
            Guid? companyExecutorId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Tickets
                .Include(t => t.Project)
                .AsQueryable();

            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId.Value);

            if (companyRequestId.HasValue)
                query = query.Where(t => t.Project != null && t.Project.CompanyRequestId == companyRequestId.Value);

            if (companyExecutorId.HasValue)
                query = query.Where(t => t.Project != null && t.Project.CompanyId == companyExecutorId.Value);

            var tickets = await query.ToListAsync(cancellationToken);

            var statusCount = tickets
                 .GroupBy(t => t.status ?? "Unknown")
                 .ToDictionary(g => g.Key, g => g.Count());

            return new TicketStatusCountResponse
            {
                StatusCounts = statusCount,
                Total = tickets.Count
            };
        }


        public async Task<Ticket?> AcceptTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
                return null;

            if (ticket.status != TicketStatusEnum.Pending.ToString())
                throw new InvalidOperationException("Only tickets with status Pending can be accepted.");

            ticket.status = TicketStatusEnum.Accepted.ToString();
            ticket.UpdatedAt = DateTime.UtcNow.AddHours(7);

            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync(cancellationToken);
            return ticket;
        }

        public async Task<Ticket?> RejectTicketAsync(Guid ticketId, string? reason = null, CancellationToken cancellationToken = default)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
                return null;

            if (ticket.status != TicketStatusEnum.Pending.ToString())
                throw new InvalidOperationException("Only tickets with status Pending can be rejected.");

            ticket.status = TicketStatusEnum.Rejected.ToString();
            ticket.reason = reason;
            ticket.UpdatedAt = DateTime.UtcNow.AddHours(7);

            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync(cancellationToken);
            return ticket;
        }

    }
}
