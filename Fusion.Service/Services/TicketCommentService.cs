using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TicketComment;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.TicketComment;
using Fusion.Service.ViewModels.Tickets.Responses;

namespace Fusion.Service.Services
{
    public class TicketCommentService : ITicketCommentService
    {
        private readonly ITicketCommentRepository _repository;
        private readonly IMapper _mapper;
        private readonly IValidator<TicketComment> _validator;

        public TicketCommentService(
            ITicketCommentRepository repository,
            IMapper mapper,
            IValidator<TicketComment> validator)
        {
            _repository = repository;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<PagedResult<TicketCommentResponse>> GetCommentsByTicketIdAsync(Guid userId,
            TicketCommentPagedRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _repository.GetCommentsByTicketIdAsync(userId, request, cancellationToken);
            var mapped = result.Items
               .Select(tc => _mapper.Map<TicketCommentResponse>(
                   tc,
                   opt => opt.Items["CurrentUserId"] = userId))
               .ToList();

            return new PagedResult<TicketCommentResponse>
            {
                Items = mapped,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        public async Task<TicketCommentResponse?> GetByIdAsync(Guid UserId,long id)
        {
            var comment = await _repository.GetByIdAsync(UserId, id);
            if (comment == null) return null;

            return _mapper.Map<TicketCommentResponse>(
                comment,
                opt => opt.Items["CurrentUserId"] = UserId);
        }

        public async Task<TicketCommentResponse> AddCommentAsync(TicketComment comment, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateAndThrowAsync(comment, cancellationToken: cancellationToken);
            var newComment = await _repository.AddAsync(comment, cancellationToken);
            return _mapper.Map<TicketCommentResponse>(
             newComment,
             opt => opt.Items["CurrentUserId"] = comment.AuthorUserId
         );
        }


        public async Task<TicketCommentResponse> UpdateCommentAsync(Guid userId, TicketComment comment, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateAndThrowAsync(comment, cancellationToken: cancellationToken);

            var updated = await _repository.UpdateAsync(userId, comment, cancellationToken);

            return _mapper.Map<TicketCommentResponse>(
                updated,
                opt => opt.Items["CurrentUserId"] = comment.AuthorUserId
            );
        }

        public async Task<bool?> DeleteCommentAsync(Guid UserId,long commentId, CancellationToken cancellationToken = default)
        {
            var comment = await _repository.GetByIdAsync(UserId, commentId);
            if (comment == null) return false;

            await _repository.DeleteAsync(UserId, comment, cancellationToken);
            return true;
        }


    }
}
