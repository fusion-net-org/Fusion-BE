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

		public TicketService(IMapper mapper, ITicketRepository ticketRepository, IUserRepository userRepository, IValidator<TicketRequest> validator,
			IUnitOfWork unitOfWork, ICompanyActivityService logService, ICurrentService currentService)
		{
			_mapper = mapper;
			_ticketRepository = ticketRepository;
			_userRepository = userRepository;
			_validator = validator;
			_unitOfWork = unitOfWork;
			_logService = logService;
			_currentService	= currentService;
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

			var newTicket = await _ticketRepository.AddTicketAsync(ticket, cancellationToken);

			var companyId = await GetCompanyIdAsync(newTicket.Id);

            var currentUserName = await GetUserName(_currentService.GetUserId());
            var log = new CompanyActivityLog
			{
                CompanyId = companyId,
                ActorUserId = _currentService.GetUserId(),
                Title = "Create ticket",
                Description = $"User:{currentUserName} has created ticket '{newTicket.TicketName}' for project '{newTicket.Project.Name}'",
            };
			await _logService.CreateLog(log);
			return _mapper.Map<TicketResponse>(newTicket);
		}

		public async Task<bool?> DeleteTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
		{

			var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);

			await _ticketRepository.DeleteTicketAsync(ticket, cancellationToken);

			var companyId = await GetCompanyIdAsync(ticketId);

            var currentUserName = await GetUserName(_currentService.GetUserId());
            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _currentService.GetUserId(),
                Title = "Delete ticket",
                Description = $"User:{currentUserName} has deleted ticket '{ticket.TicketName}' from project '{ticket.Project.Name}'",
            };
			await _logService.CreateLog(log);
            return true;
		}

		public async Task<PagedResult<TicketResponse>> GetPageTicketshAsync(TicketPagedSearchRequest request, CancellationToken cancellationToken = default)
		{
			if (request == null)
				throw CustomExceptionFactory.CreateBadRequestError(
					ResponseMessages.INVALID_INPUT);

			var result = await _ticketRepository.GetPageTicketshAsync(request, cancellationToken);

			if (result == null || result.Items.Count == 0)
				throw CustomExceptionFactory.CreateNotFoundError(
					ResponseMessages.NOT_FOUND.FormatMessage("Tickets"));

			var list = new PagedResult<TicketResponse>
			{
				Items = _mapper.Map<List<TicketResponse>>(result.Items),
				TotalCount = result.TotalCount,
				PageNumber = result.PageNumber,
				PageSize = result.PageSize
			};
			return list;
		}

		public async Task<TicketResponse?> GetTicketByIdAsync(Guid id)
		{
			var ticket = await _ticketRepository.GetTicketByIdAsync(id);
			if (ticket == null)
				throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Ticket"));

			return _mapper.Map<TicketResponse>(ticket);
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

			var companyId = await GetCompanyIdAsync(ticketId);
            var currentUserName = await GetUserName(_currentService.GetUserId());
            var log = new CompanyActivityLog
			{
				CompanyId = companyId,
				ActorUserId = _currentService.GetUserId(),
				Title = "Update ticket",
				Description = $"User:{currentUserName} has updated ticket '{result.TicketName}' from project '{result.Project.Name}'",
			};
			await _logService.CreateLog(log);
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
