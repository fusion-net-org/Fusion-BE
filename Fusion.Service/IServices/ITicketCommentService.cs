using System;
using System.Threading;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TicketComment;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.TicketComment;
using Fusion.Service.ViewModels.Tickets.Responses;

namespace Fusion.Service.IServices
{
    public interface ITicketCommentService
    {
        Task<PagedResult<TicketCommentResponse>> GetCommentsByTicketIdAsync(Guid userId,
            TicketCommentPagedRequest request,
            CancellationToken cancellationToken = default);

        Task<TicketCommentResponse?> GetByIdAsync(Guid UserId, long id);

        Task<TicketCommentResponse> AddCommentAsync(TicketComment comment, CancellationToken cancellationToken = default);

        Task<TicketCommentResponse> UpdateCommentAsync(Guid UserId,TicketComment comment, CancellationToken cancellationToken = default);

        Task<bool?> DeleteCommentAsync(Guid UserId,long commentId, CancellationToken cancellationToken = default);

    }
}
