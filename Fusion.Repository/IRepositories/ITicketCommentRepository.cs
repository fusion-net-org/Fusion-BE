using System;
using System.Threading;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TicketComment;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface ITicketCommentRepository
    {
        Task<PagedResult<TicketComment>> GetCommentsByTicketIdAsync(Guid userId, TicketCommentPagedRequest request,CancellationToken cancellationToken = default);

        Task<TicketComment?> GetByIdAsync(Guid UserId,long id);
        Task<TicketComment> AddAsync(TicketComment comment, CancellationToken cancellationToken = default);
        Task<TicketComment> UpdateAsync(Guid UserId,TicketComment comment, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid UserId,TicketComment comment, CancellationToken cancellationToken = default);
    }
}
