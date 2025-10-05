using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.Comment.Request;
using Fusion.Service.ViewModels.Comment.Response;

namespace Fusion.Service.IServices
{
    public interface ICommentService
    {
        Task<CommentResponse> CreateCommentAsync(CommentRequest comment, Guid UserId);
        Task<CommentResponse?> GetCommentByIdAsync(long id);
        Task<IEnumerable<CommentResponse>> GetAllCommentsAsync();
        Task<CommentResponse?> UpdateCommentAsync(CommentRequest comment, Guid userId);
        Task<bool> DeleteCommentAsync(long id);
    }
}
