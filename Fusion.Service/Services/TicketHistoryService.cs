using AutoMapper;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.IRepositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Tickets.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    public class TicketHistoryService : ITicketHistoryService
    {
        private readonly ITicketHistoryRepository _repository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IMapper _mapper;

        public TicketHistoryService(
            ITicketHistoryRepository repository,
            ITicketRepository ticketRepository,
            IMapper mapper)
        {
            _repository = repository;
            _ticketRepository = ticketRepository;
            _mapper = mapper;
        }

        public async Task<List<TicketHistoryResponse>> GetByTicketIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket not found");

            var histories = await _repository.GeTTicketHistoryByTicketIdAsync(ticketId, cancellationToken);
            return _mapper.Map<List<TicketHistoryResponse>>(histories);
        }

        public async Task<PagedResult<TicketHistoryResponse>> GetTicketHistoryByTicketIdAsync(TicketHistoryPagedRequest request,CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(request.TicketId);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket not found");

            var histories = await _repository.GetTicketHistoryByTicketIdAsync(request, cancellationToken);

            var mapped = _mapper.Map<List<TicketHistoryResponse>>(histories.Items);

            return new PagedResult<TicketHistoryResponse>
            {
                Items = mapped,
                TotalCount = histories.TotalCount,
                PageNumber = histories.PageNumber,
                PageSize = histories.PageSize
            };
        }

    }

}
