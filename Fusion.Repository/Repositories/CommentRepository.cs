using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class CommentRepository : GenericRepository<Comment>, ICommentRepository
    {
        private readonly FusionDbContext _context;
        public CommentRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Comment> CreateCommentAsync(Comment comment, Guid UserId)
        {
            var task = await _context.ProjectTasks.FindAsync(comment.TaskId);

            if (task == null)
                throw CustomExceptionFactory.
                    CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Task"));

            comment.CreateAt = DateTime.UtcNow.AddHours(7);
            comment.AuthorUserId = UserId;
            comment.Status = "Active";

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(long id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
                throw CustomExceptionFactory.
                                    CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Comment"));
            
            if(comment.Status.ToLower() == "Inactive".ToLower())
            {
                throw CustomExceptionFactory.
                                   CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Comment"));
            }

            comment.Status = "Inactive";
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
        {
             return await _context.Comments
                .Include(x => x.Task)
                .Where(x => x.Status.ToLower() == "Active".ToLower())
                .ToListAsync();          
        }

        public async Task<Comment?> GetCommentByIdAsync(long id)
        {
            var commentById = await _context.Comments
                .Include(x => x.Task)
                .FirstOrDefaultAsync(c => c.Id == id);

            if(commentById == null)
                throw CustomExceptionFactory.
                                    CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Comment"));
            if (commentById.Status.ToLower() == "Inactive".ToLower())
            {
                throw CustomExceptionFactory.
                                   CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Comment"));
            }

            return commentById;
        }

        public async Task<Comment?> UpdateCommentAsync(Comment comment, Guid userId)
        {
            var existingComment = await _context.Comments.FindAsync(comment.Id);
            if (existingComment == null)
                throw CustomExceptionFactory.
                                    CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Comment"));

            if (existingComment.Status.ToLower() == "Inactive".ToLower())
            {
                throw CustomExceptionFactory.
                                   CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Comment"));
            }

            existingComment.AuthorUserId = userId;
            existingComment.Body = comment.Body;
            existingComment.UpdateAt = DateTime.UtcNow.AddHours(7);

            _context.Comments.Update(existingComment);
            await _context.SaveChangesAsync(); 
            return existingComment;
        }
    }
}
