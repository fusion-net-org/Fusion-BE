using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface ICommentRepository
    {
        Task<Comment> CreateCommentAsync(Comment comment, Guid UserId);
        Task<Comment?> GetCommentByIdAsync(long id);
        Task<IEnumerable<Comment>> GetAllCommentsAsync();
        Task<Comment?> UpdateCommentAsync(Comment comment, Guid userId);
        Task<bool> DeleteCommentAsync(long id);
    }
}
