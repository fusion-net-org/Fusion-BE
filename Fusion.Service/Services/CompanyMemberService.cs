using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company_Member;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Email;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Notifications.Requests;
using Fusion.Service.ViewModels.Permission.Responses;
using Fusion.Service.ViewModels.Role.Responses;
using Fusion.Service.ViewModels.UserRole.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Travelogue.Repository.Caching;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Fusion.Service.Services
{
    public class CompanyMemberService : ICompanyMemberService
    {

        private readonly ICompanyMemberRepository _companyMemberRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMailService _mailService;
        private readonly IMapper _mapper;
        private readonly ICompanyActivityService _logService;
        private readonly ICurrentService _currentService;

        public CompanyMemberService(ICompanyMemberRepository companyMemberRepository, IProjectMemberRepository projectMemberRepository,
            INotificationService notificationService, IUserRepository userRepository, ICompanyRepository companyRepository,
            IMapper mapper, IMailService mailService, ICompanyActivityService logService, ICurrentService currentService)
        {

            _companyMemberRepository = companyMemberRepository;
            _projectMemberRepository = projectMemberRepository;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _mailService = mailService;
            _mapper = mapper;
            _logService = logService;
            _currentService = currentService;
        }

        public async Task<CompanyMemberResponse?> FiredMemberFromCompany(string terminatorEmail, string firedMemberMail, string reason, Guid companyId, CancellationToken token = default)
        {
            var result = await _companyMemberRepository.FiredMemberFromCompany(terminatorEmail, firedMemberMail, reason, companyId, token);

            var response = await _companyMemberRepository.GetCompanyMemberByIdAsync(result.Id);

            var emailBody = MailUtils.FiredMemberFromCompany(
                               terminatorEmail,
                               result.User.UserName,
                               result.Company.Name,
                               reason);

            await _mailService.SendEmailAsync(new MailRequest()
            {
                Subject = $"{result.User.UserName} has been fired from {result.Company.Name}",
                Body = emailBody,
                ToEmail = result.User.Email
            });

            //await _notificationService.CreateNotificationAsync(new SendNotificationRequest
            //{
            //    UserId = result.UserId.Value,
            //    Title = "Fired from company",
            //    Body = $"You have been removed from the company {response.Company.Name} by {response.Company.OwnerUser.UserName}. Reason: {reason}",
            //    LinkKey = null,
            //    IdLink = null,
            //    Event = "MEMBER_REMOVED",
            //    NotificationType = NotificationTypeEnum.BUSINESS.ToString()
            //});
            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _currentService.GetUserId(),
                Title = "Fired Member From Company",
                Description = $"User id:'{_currentService.GetUserId()}'  deleted member with user id '{result.User.Id}' has left the company .",

            };
            return _mapper.Map<CompanyMemberResponse>(response);
        }

        public async Task<PagedResult<CompanyMemberResponse>> GetPagedCompanyMemberByCompanyIdAsync(Guid companyId, string mail, CompanyMemberPagedSearchRequest request, CancellationToken token = default)
        {

            var result = await _companyMemberRepository.GetPagedCompanyMemberByCompanyIdAsync(companyId, mail, request, token);

            var memberResponses = new List<CompanyMemberResponse>();
            foreach (var member in result.Items)
            {
                var numberProjectJoin = await _projectMemberRepository.GetTotalProjectsForMemberInCompanyAsync(member.User.Id, companyId, token);

                var dto = _mapper.Map<CompanyMemberResponse>(member);
                dto.NumberProductJoin = numberProjectJoin;

                memberResponses.Add(dto);
            }

            return new PagedResult<CompanyMemberResponse>
            {
                Items = memberResponses,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        public async Task<PagedResult<CompanyMemberResponse>> GetPagedCompanyMemberAsync(CompanyMemberPagedSearchAdminRequest request, CancellationToken token = default)
        {

            var result = await _companyMemberRepository.GetPagedCompanyMemberAsync(request, token);

            var memberResponses = new List<CompanyMemberResponse>();
            foreach (var member in result.Items)
            {
                var numberProjectJoin = await _projectMemberRepository.GetTotalProjectsForMemberAsync(member.User.Id, token);

                var dto = _mapper.Map<CompanyMemberResponse>(member);
                dto.NumberProductJoin = numberProjectJoin;

                memberResponses.Add(dto);
            }

            return new PagedResult<CompanyMemberResponse>
            {
                Items = memberResponses,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        public async Task<CompanyMemberResponse?> InviteMemberToCompany(string inviterEmail, string inviteeMemberMail, Guid companyId, CancellationToken cancellationToken)
        {
            var result = await _companyMemberRepository.InviteMemberToCompany(inviterEmail, inviteeMemberMail, companyId, cancellationToken);

            var response = await _companyMemberRepository.GetCompanyMemberByIdAsync(result.Id);

            //await _notificationService.CreateNotificationAsync(new ViewModels.Notifications.Requests.SendNotificationRequest
            //{
            //    UserId = response.UserId.Value,
            //    Title = "You have been invited to join a company",
            //    Body = $"{response.Company.OwnerUser.UserName} has invited you to join the company {response.Company.Name}.",
            //    LinkKey = null,
            //    IdLink = null,
            //    Event = "Invite_Member",
            //    Context = null,
            //    NotificationType = NotificationTypeEnum.BUSINESS.ToString(),
            //});

            var owner_user = await _userRepository.GetUserByEmailAsync(inviterEmail);

            var inviteToken = result.Id.ToString();

            //var cacheKey = $"company_invite:{inviteToken}";

            //await _cacheService.SetAsync(cacheKey,
            //    new InviteMemberRequest
            //    {
            //        CompanyId = company.Id,
            //        InviteeMemberId = inviteeMemberId
            //    },
            //    TimeSpan.FromMinutes(10), cancellationToken);

            var inviteUrl = $"https://localhost:7160/api/companymember/accept?tokenConfirm={inviteToken}";
            var rejectUrl = $"https://localhost:7160/api/companymember/reject?tokenConfirm={inviteToken}";

            var emailBody = MailUtils.InviteMemberToCompany(
                                owner_user.UserName,
                                result.User.UserName,
                                result.Company.Name,
                                inviteUrl,
                                rejectUrl,
                                10);

            await _mailService.SendEmailAsync(new MailRequest()
            {
                Subject = $"Welcome {result.User.UserName} joined our {result.Company.Name}!",
                Body = emailBody,
                ToEmail = result.User.Email
            });

            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = _currentService.GetUserId(),
                Title = "Invite Member To Company",
                Description = $"User id:'{_currentService.GetUserId()}'  sent a company invitation to user with ID {result.User.Id}'.",

            };
            await _logService.CreateLog(log, cancellationToken);
            return _mapper.Map<CompanyMemberResponse>(response);

        }

        public async Task<CompanyMemberResponse?> AcceptJoinMemberToCompany(string tokenConfirm, CancellationToken cancellationToken = default)
        {
            //var cacheKey = $"company_invite:{tokenConfirm}";
            //var data = await _cacheService.GetAsync<InviteMemberRequest>(cacheKey, cancellationToken);

            var data = await _companyMemberRepository.GetCompanyMemberByIdAsync(long.Parse(tokenConfirm));

            if (data == null)
                throw CustomExceptionFactory.
                   CreateNotFoundError("Token is invalid");

            var result = await _companyMemberRepository.AcceptJoinMemberToCompany(data.UserId.Value, data.CompanyId.Value, cancellationToken);

            var log = new CompanyActivityLog
            {
                CompanyId = data.CompanyId.Value,
                ActorUserId = data.UserId.Value,
                Title = "Accept Join Member To Company",
                Description = $"User id:'{data.UserId.Value}' has agreed to join the company.",

            };
            await _logService.CreateLog(log, cancellationToken);
            return _mapper.Map<CompanyMemberResponse>(result);

        }

        public async Task<CompanyMemberResponse?> RejectJoinMemberToCompany(string tokenConfirm, CancellationToken cancellationToken = default)
        {
            //var cacheKey = $"company_invite:{tokenConfirm}";
            //var data = await _cacheService.GetAsync<InviteMemberRequest>(cacheKey, cancellationToken);

            var data = await _companyMemberRepository.GetCompanyMemberByIdAsync(long.Parse(tokenConfirm));

            if (data == null)
                throw CustomExceptionFactory.
                   CreateNotFoundError("Token is invalid");

            var result = await _companyMemberRepository.RejectJoinMemberToCompany(data.UserId.Value, data.CompanyId.Value, cancellationToken);

            var log = new CompanyActivityLog
            {
                CompanyId = data.CompanyId.Value,
                ActorUserId = data.UserId.Value,
                Title = "Reject Join Member To Company",
                Description = $"User id:'{data.UserId.Value}' has refured to join the company.",

            };
            await _logService.CreateLog(log, cancellationToken);
            return _mapper.Map<CompanyMemberResponse>(result);
        }

        public async Task<CompanyMemberResponse?> RemoveMemberFromCompany(string terminatorEmail, Guid userId, Guid companyId, CancellationToken token = default)
        {
            var result = await _companyMemberRepository.RemoveMemberFromCompany(terminatorEmail, userId, companyId, token);

            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId =_currentService.GetUserId(),
                Title = "Remove Member From Company",
                Description = $"User id:'{_currentService.GetUserId()}' deleted member with user id '{userId}' has left the company.",

            };
            await _logService.CreateLog(log);
            return _mapper.Map<CompanyMemberResponse>(result);
        }

        public async Task<List<CompanyMemberResponse>> GetMembersByStatus(Guid companyId, string status, CancellationToken token = default)
        {
            var members = await _companyMemberRepository.GetMembersByStatus(companyId, status, token);
            return _mapper.Map<List<CompanyMemberResponse>>(members);
        }

        public async Task<Dictionary<string, int>> GetSummaryStatusByCompanyId(Guid companyId, CancellationToken token = default)
        {
            return await _companyMemberRepository.GetSummaryStatusByCompanyId(companyId, token);
        }

        public async Task<AddMemberRoleInCompanyResponse?> AddRoleForMemberInCompany(Guid companyId, List<int> roleIds, Guid memberId, string inviterEmail, CancellationToken token)
        {
            var addRole = await _companyMemberRepository.AddRoleForMemberInCompany(companyId, roleIds, memberId, inviterEmail, token); 

            if(!addRole.Any())
                 throw CustomExceptionFactory.CreateBadRequestError("Add Role in Company Fail");

            var userWithRoles = await _userRepository.GetUserWithRolesAndPermissionsInCompanyAsync(memberId, companyId);

            if (userWithRoles == null)
                throw CustomExceptionFactory.CreateNotFoundError("User not found after adding roles");

            var response = new AddMemberRoleInCompanyResponse
            {
                UserId = userWithRoles.Id,
                UserName = userWithRoles.UserName ?? "",
                Roles = userWithRoles.UserRoles.Select(ur => new RoleResponse
                {
                    RoleId = ur.Role.Id,
                    RoleName = ur.Role.RoleName ?? "",
                    Permissions = ur.Role.RolePermissions.Select(rp => new PermissionResponse
                    {
                        FunctionCode = rp.Function.FunctionCode ?? "",
                        FunctionName = rp.Function.FunctionName ?? "",
                        PageCode = rp.Function.PageCode ?? "",
                        IsAccess = rp.IsAccess
                    }).ToList()
                }).ToList()
            };

            return response;
        }
    }
}
