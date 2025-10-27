using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Comment.Request;
using Fusion.Service.ViewModels.Comment.Response;
using Fusion.Service.ViewModels.Task.Response;

namespace Fusion.Service.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;

        public CommentService(ICommentRepository commentRepository, IMapper mapper)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
        }

        public async Task<CommentResponse> CreateCommentAsync(CommentRequest comment, Guid UserId)
        {
            var entity = _mapper.Map<Comment>(comment);

            var created = await _commentRepository.CreateCommentAsync(entity, UserId);

            return _mapper.Map<CommentResponse>(created);
        }

        public async Task<bool> DeleteCommentAsync(long id)
        {
            return await _commentRepository.DeleteCommentAsync(id);
        }

        public async Task<IEnumerable<CommentResponse>> GetAllCommentsAsync()
        {
            var entity = await _commentRepository.GetAllCommentsAsync();

            return _mapper.Map<IEnumerable<CommentResponse>>(entity);
        }

        public async Task<CommentResponse?> GetCommentByIdAsync(long id)
        {
            var entity = await _commentRepository.GetCommentByIdAsync(id);

            return _mapper.Map<CommentResponse?>(entity);
        }

        public async Task<CommentResponse?> UpdateCommentAsync(CommentRequestUpdate comment, Guid userId)
        {
            var entity = _mapper.Map<Comment>(comment);
            var updated = await _commentRepository.UpdateCommentAsync(entity, userId);
            return _mapper.Map<CommentResponse?>(updated);
        }
    }
}
