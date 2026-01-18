using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TicketComment;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.Repository.Repositories
{
    public class TicketCommentRepository : GenericRepository<TicketComment>, ITicketCommentRepository
    {
        private readonly FusionDbContext _context;
        private readonly ITicketHistoryRepository _ticketHistoryRepository;

        public TicketCommentRepository(FusionDbContext context, ITicketHistoryRepository ticketHistoryRepository) : base(context)
        {
            _context = context;
            _ticketHistoryRepository = ticketHistoryRepository;
        }

        public async Task<PagedResult<TicketComment>> GetCommentsByTicketIdAsync(
             Guid userId,
             TicketCommentPagedRequest request,
             CancellationToken cancellationToken = default)
        {
            var query = _context.TicketComments
                .Include(c => c.AuthorUser)
                .Where(c => c.TicketId == request.TicketId && c.IsDeleted == false)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                query = query.Where(c => c.Body.Contains(request.SearchText));
            }
            if (request.From.HasValue)
            {
                var fromDate = request.From.Value.Date; 
                query = query.Where(c => c.CreateAt.Date >= fromDate);
            }

            if (request.To.HasValue)
            {
                var toDate = request.To.Value.Date; 
                query = query.Where(c => c.CreateAt.Date <= toDate);
            }


            return await query.ToPagedResultAsync(request, cancellationToken);
        }
        public async Task<TicketComment?> GetByIdAsync(Guid UserId, long id)
        {
            return await _context.TicketComments
                .Include(c => c.AuthorUser)
                .SingleOrDefaultAsync(c => c.Id == id
                                           && c.AuthorUserId == UserId);
        }


        public async Task<TicketComment> AddAsync(TicketComment comment, CancellationToken cancellationToken = default)
        {
            comment.CreateAt = DateTime.UtcNow.AddHours(7);
            comment.UpdateAt = DateTime.UtcNow.AddHours(7);
            comment.IsDeleted = false;

            var entity = await _context.TicketComments.AddAsync(comment, cancellationToken);

            await _ticketHistoryRepository.AddAsync(new TicketHistory
            {
                TicketId = comment.TicketId!.Value,
                Action = TicketHistoryAction.CommentAdded.ToString(),
                Description = "New comment added",
                PerformedBy = comment.AuthorUserId
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            return entity.Entity;
        }

        public async Task<TicketComment> UpdateAsync(Guid UserId, TicketComment comment, CancellationToken cancellationToken = default)
        {
            var existing = await _context.TicketComments.FindAsync(comment.Id);
            if (existing == null) throw new KeyNotFoundException();


            if (existing.AuthorUserId != UserId)
                throw new UnauthorizedAccessException("You are not allowed to update this comment");


            existing.Body = comment.Body;
            existing.UpdateAt = DateTime.UtcNow.AddHours(7);

            await _ticketHistoryRepository.AddAsync(new TicketHistory
            {
                TicketId = existing.TicketId!.Value,
                Action = TicketHistoryAction.CommentUpdated.ToString(),
                Description = "Comment updated",
                PerformedBy = UserId
            }, cancellationToken);


            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }


        public async Task<bool> DeleteAsync(Guid UserId,TicketComment comment, CancellationToken cancellationToken = default)
        {
            var existing = await _context.TicketComments.FindAsync(comment.Id);
            if (existing == null) return false;

            if (existing.AuthorUserId != UserId)
                throw new UnauthorizedAccessException("You are not allowed to delete this comment");


            comment.IsDeleted = true;
            _context.TicketComments.Update(comment);

            await _ticketHistoryRepository.AddAsync(new TicketHistory
            {
                TicketId = existing.TicketId!.Value,
                Action = TicketHistoryAction.CommentDeleted.ToString(),
                Description = "Comment deleted",
                PerformedBy = UserId
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> RestoreAsync(TicketComment comment, CancellationToken cancellationToken = default)
        {
            comment.IsDeleted = false;
            _context.TicketComments.Update(comment);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
