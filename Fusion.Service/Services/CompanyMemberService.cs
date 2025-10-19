using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
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
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Travelogue.Repository.Caching;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Fusion.Service.Services
{
    public class CompanyMemberService : ICompanyMemberService
    {

        private readonly ICompanyMemberRepository _companyMemberRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;

        public CompanyMemberService(ICompanyMemberRepository companyMemberRepository, IProjectMemberRepository projectMemberRepository, INotificationService notificationService, IUserRepository userRepository, ICompanyRepository companyRepository, IMapper mapper)
        {

            _companyMemberRepository = companyMemberRepository;
            _projectMemberRepository = projectMemberRepository;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _mapper = mapper;
        }

        public async Task<CompanyMemberResponse?> FiredMemberFromCompany(string terminatorEmail, Guid firedMemberId, Guid companyId, CancellationToken token = default)
        {
            var result = await _companyMemberRepository.FiredMemberFromCompany(terminatorEmail, firedMemberId, companyId, token);

            var response = await _companyMemberRepository.GetCompanyMemberByIdAsync(result.Id);

            //await _notificationService.CreateNotificationAsync(new SendNotificationRequest
            //{
            //    UserId = firedMemberId,
            //    Title = "Fired from company",
            //    Body = $"You have been removed from the company {response.Company.Name} by {response.Company.OwnerUser.UserName}.",
            //    LinkKey = null,
            //    IdLink = null,
            //    Event = "MEMBER_REMOVED",
            //    NotificationType = NotificationTypeEnum.BUSINESS.ToString()
            //});

            return _mapper.Map<CompanyMemberResponse>(response);
        }

        public async Task<PagedResult<CompanyMemberResponse>> GetPagedCompanyMemberByCompanyIdAsync(Guid companyId, string mail, PagedRequest request, CancellationToken token = default)
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

        public async Task<CompanyMemberResponse?> InviteMemberToCompany(string inviterEmail, Guid inviteeMemberId, Guid companyId, CancellationToken cancellationToken)
        {
            var result = await _companyMemberRepository.InviteMemberToCompany(inviterEmail, inviteeMemberId, companyId, cancellationToken);

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

            return _mapper.Map<CompanyMemberResponse>(response);

            #region Tui tính để đó khi nào làm ProjectMember mới dùng tới
            //if (check_member is not true)
            //    return check_member;

            //var owner_user = await _userRepository.GetUserByEmailAsync(inviterEmail);
            //var member = await _userRepository.GetUserByIdAsync(inviteeMemberId);
            //var company = await _companyRepository.GetCompanyByIdAsync(companyId);

            //var inviteToken = Guid.NewGuid().ToString();

            //var cacheKey = $"company_invite:{inviteToken}";

            //await _cacheService.SetAsync(cacheKey,
            //    new InviteMemberRequest
            //    {
            //        CompanyId = company.Id,
            //        InviteeMemberId = inviteeMemberId
            //    },
            //    TimeSpan.FromMinutes(10), cancellationToken);

            //var inviteUrl = $"https://localhost:7160/api/companymember/join?tokenConfirm={inviteToken}"; // hoặc token thật
            //var emailBody = MailUtils.InviteMemberToCompany(
            //                    owner_user.UserName,
            //                    member.UserName,
            //                    company.Name,
            //                    inviteUrl,
            //                    10);

            //await _mailService.SendEmailAsync(new MailRequest()
            //{
            //    Subject = $"Chào mừng bạn đến với {company.Name}!",
            //    Body = emailBody,
            //    ToEmail = member.Email
            //});

            //    public async Task<CompanyMemberResponse?> JoinMemberToCompany(string tokenConfirm, CancellationToken cancellationToken = default)
            //{
            //    var cacheKey = $"company_invite:{tokenConfirm}";
            //    var data = await _cacheService.GetAsync<InviteMemberRequest>(cacheKey, cancellationToken);

            //    if (data is null)
            //        throw CustomExceptionFactory.
            //           CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Token is invalid or expired"));

            //    var result = await _companyMemberRepository.JoinMemberToCompany(data.InviteeMemberId, data.CompanyId, cancellationToken);
            //    return _mapper.Map<CompanyMemberResponse>(result);
            //}
            #endregion

        }




    }
}
