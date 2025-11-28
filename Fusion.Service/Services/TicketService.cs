using AutoMapper;
using FluentValidation;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Ticket;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Tickets.Requests;
using Fusion.Service.ViewModels.Tickets.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Fusion.Service.Services
{
    public class TicketService : ITicketService
    {
        private readonly IMapper _mapper;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly IValidator<TicketRequest> _validator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompanyActivityService _logService;
        private readonly ICurrentService _currentService;
        private readonly IProjectService _projectService;
        public TicketService(IMapper mapper, ITicketRepository ticketRepository, IUserRepository userRepository, IValidator<TicketRequest> validator,
            IUnitOfWork unitOfWork, ICompanyActivityService logService, ICurrentService currentService, IProjectService projectService)
        {
            _mapper = mapper;
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _validator = validator;
            _unitOfWork = unitOfWork;
            _logService = logService;
            _currentService = currentService;
            _projectService = projectService;
        }

        public async Task<TicketResponse?> CreateTicketAsync(TicketRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT);

            await _validator.ValidateAndThrowAsync(
               request,
               opts => opts.IncludeRuleSets("Create"),
               cancellationToken
               );



            var ticket = _mapper.Map<Ticket>(request);

            ticket.SubmittedBy = request.SubmittedBy;


            var newTicket = await _ticketRepository.AddTicketAsync(ticket, cancellationToken);

            //var companyId = await GetCompanyIdAsync(newTicket.Id);

            //         var currentUserName = await GetUserName(_currentService.GetUserId());
            //         var log = new CompanyActivityLog
            //{
            //             CompanyId = companyId,
            //             ActorUserId = _currentService.GetUserId(),
            //             Title = "Create ticket",
            //             Description = $"User:{currentUserName} has created ticket '{newTicket.TicketName}' for project '{newTicket.Project.Name}'",
            //         };
            //await _logService.CreateLog(log);
            return _mapper.Map<TicketResponse>(newTicket);
        }

        public async Task<bool?> DeleteTicketAsync(Guid ticketId, string reason, CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket not found");

            var currentUserId = _currentService.GetUserId();

            if (ticket.SubmittedBy != currentUserId)
                throw new UnauthorizedAccessException("You are not allowed to delete this ticket");

            await _ticketRepository.DeleteTicketAsync(ticket, reason, cancellationToken);
            return true;
        }


        public async Task<TicketPagedResponse> GetPageTicketshAsync(
            TicketPagedSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            var fullQuery = _ticketRepository.BuildTicketQuery(request);

            var fullData = await fullQuery.ToListAsync(cancellationToken);

            var statusCounts = fullData
                .GroupBy(t => t.status ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            var totalFull = fullData.Count;

            var paged = await fullQuery.ToPagedResultAsync(request, cancellationToken);

            return new TicketPagedResponse
            {
                PageData = new PagedResult<TicketResponse>
                {
                    Items = _mapper.Map<List<TicketResponse>>(paged.Items),
                    TotalCount = paged.TotalCount,
                    PageNumber = paged.PageNumber,
                    PageSize = paged.PageSize
                },
                StatusCounts = statusCounts,
                Total = totalFull
            };
        }




        public async Task<TicketResponse?> GetTicketByIdAsync(Guid id)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(id);
            if (ticket == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Ticket"));

            return _mapper.Map<TicketResponse>(ticket);
        }

        public async Task<TicketDashboardResponse> GetTicketDashboardAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var tickets = await _ticketRepository.GetTicketsForDashboardAsync(projectId, cancellationToken);
            var dashboard = new TicketDashboardResponse();

            // Ticket Status
            var statusInProgress = tickets.Count(t => t.Status != null && t.Status.IsStart);
            var statusResolved = tickets.Count(t => t.Status != null && t.Status.IsEnd);
            dashboard.TicketStatusData.Add(new TicketStatusChartItem { Name = "In Progress", Value = statusInProgress });
            dashboard.TicketStatusData.Add(new TicketStatusChartItem { Name = "Resolved", Value = statusResolved });

            // Budget theo Priority
            dashboard.BudgetByPriority = tickets
                .GroupBy(t => t.Priority ?? "Unknown")
                .Select(g => new BudgetByPriorityItem { Status = g.Key, Budget = g.Sum(t => t.Budget ?? 0) })
                .ToList();

            // Số lượng Priority
            dashboard.TicketPriorityData = tickets
                .GroupBy(t => t.Priority ?? "Unknown")
                .Select(g => new TicketPriorityChartItem { Priority = g.Key, Value = g.Count() })
                .ToList();

            // Số lượng Resolved và Closed
            var resolved = tickets.Count(t => t.ResolvedAt.HasValue);
            var closed = tickets.Count(t => t.ClosedAt.HasValue);
            dashboard.ResolvedAndClosedData.Add(new ResolvedClosedChartItem { Name = "Resolved", Value = resolved });
            dashboard.ResolvedAndClosedData.Add(new ResolvedClosedChartItem { Name = "Closed", Value = closed });

            // Resolved & Closed timeline theo tuần
            dashboard.ResolvedClosedTimeline = tickets
                .Where(t => t.ResolvedAt.HasValue || t.ClosedAt.HasValue)
                .GroupBy(t => t.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new ResolvedClosedTimelineItem
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Resolved = g.Count(t => t.ResolvedAt.HasValue),
                    Closed = g.Count(t => t.ClosedAt.HasValue)
                })
                .ToList();

            return dashboard;
        }


        public async Task<PagedResult<TicketResponse>> GetTicketsByProjectIdAsync(TicketByProjectPagedRequest request, CancellationToken cancellationToken = default)
        {
            var tickets = await _ticketRepository.GetTicketsByProjectIdAsync(request, cancellationToken);

            var mapped = _mapper.Map<List<TicketResponse>>(tickets.Items);

            return new PagedResult<TicketResponse>
            {
                Items = mapped,
                TotalCount = tickets.TotalCount,
                PageNumber = tickets.PageNumber,
                PageSize = tickets.PageSize
            };
        }
        public async Task<bool?> RestoreTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket not found");

            var currentUserId = _currentService.GetUserId();

            if (ticket.SubmittedBy != currentUserId)
                throw new UnauthorizedAccessException("You are not allowed to restore this ticket");

            if ((bool)!ticket.IsDeleted)
                throw new InvalidOperationException("Ticket is not deleted");

            await _ticketRepository.RestoreTicketAsync(ticket, cancellationToken);
            return true;
        }
        public async Task<TicketStatusCountResponse> GetTicketStatusCountAsync(
              Guid? projectId = null,
              Guid? companyRequestId = null,
              Guid? companyExecutorId = null,
              CancellationToken cancellationToken = default)
        {
            var result = await _ticketRepository.GetTicketStatusCountAsync(
                projectId,
                companyRequestId,
                companyExecutorId,
                cancellationToken
            );

            return result;
        }

        public async Task<TicketResponse?> AcceptTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket not found");

            //var currentUserId = _currentService.GetUserId();
            //if (ticket.SubmittedBy != currentUserId)
            //    throw new UnauthorizedAccessException("You are not allowed to accept this ticket");

            var updatedTicket = await _ticketRepository.AcceptTicketAsync(ticketId, cancellationToken);
            return _mapper.Map<TicketResponse>(updatedTicket);
        }

        public async Task<TicketResponse?> RejectTicketAsync(Guid ticketId, string? reason = null, CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket not found");

            //var currentUserId = _currentService.GetUserId();
            //if (ticket.SubmittedBy != currentUserId)
            //    throw new UnauthorizedAccessException("You are not allowed to reject this ticket");

            var updatedTicket = await _ticketRepository.RejectTicketAsync(ticketId, reason, cancellationToken);
            return _mapper.Map<TicketResponse>(updatedTicket);
        }

        public async Task<TicketResponse?> UpdateTicketAsync(TicketRequest request, Guid ticketId, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

            await _validator.ValidateAndThrowAsync(
                request,
                opts => opts.IncludeRuleSets("Update"),
                cancellationToken);

            var result = await _ticketRepository.UpdateTicketAsync(ticketId, _mapper.Map<Ticket>(request), cancellationToken);


            return _mapper.Map<TicketResponse>(result);
        }

        private async Task<Guid> GetCompanyIdAsync(Guid id)
        {
            var project = await _unitOfWork.Repository<Project>().FindAsync(c => c.Id == id);
            var company = await _unitOfWork.Repository<Company>().FindAsync(c => c.Id == project.CompanyId);

            return company.Id;
        }
        private async Task<string?> GetUserName(Guid userId)
        {
            var user = await _unitOfWork.Repository<User>().FindAsync(c => c.Id == userId);
            return user.UserName;
        }
   

    }
}
