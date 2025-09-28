using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Email;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Travelogue.Repository.Caching;

namespace Fusion.Service.Services
{
    public class CompanyMemberService : ICompanyMemberService
    {
        private readonly IMailService _mailService;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyMemberRepository _companyMemberRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;

        public CompanyMemberService(IMailService mailService, IUserRepository userRepository, ICompanyMemberRepository companyMemberRepository, ICompanyRepository companyRepository, ICacheService cacheService, IMapper mapper)
        {
            _mailService = mailService;
            _userRepository = userRepository;
            _companyMemberRepository = companyMemberRepository;
            _companyRepository = companyRepository;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        public async Task<bool?> InviteMemberToCompany(string inviterEmail, Guid inviteeMemberId, Guid companyId, CancellationToken token)
        {
            var check_member = await _companyMemberRepository.InviteMemberToCompany(inviterEmail, inviteeMemberId, companyId, token);

            if (check_member is not true)
                return check_member;

            var owner_user = await _userRepository.GetUserByEmailAsync(inviterEmail);
            var member = await _userRepository.GetUserByIdAsync(inviteeMemberId);
            var company = await _companyRepository.GetCompanyByIdAsync(companyId);

            var inviteToken = Guid.NewGuid().ToString();

            var cacheKey = $"company_invite:{inviteToken}";

            await _cacheService.SetAsync(cacheKey,
                new InviteMemberRequest
                {
                    CompanyId = company.Id,
                    InviteeMemberId = inviteeMemberId
                },
                TimeSpan.FromMinutes(10), token);

            var inviteUrl = $"https://localhost:7160/api/companymember/join?tokenConfirm={inviteToken}"; // hoặc token thật
            var emailBody = MailUtils.InviteMemberToCompany(
                                owner_user.UserName,
                                member.UserName,
                                company.Name,
                                inviteUrl,
                                10);

            await _mailService.SendEmailAsync(new MailRequest()
            {
                Subject = $"Chào mừng bạn đến với {company.Name}!",
                Body = emailBody,
                ToEmail = member.Email
            });

            return true;
        }

        public async Task<CompanyMemberResponse?> JoinMemberToCompany(string tokenConfirm, CancellationToken token = default)
        {
            var cacheKey = $"company_invite:{token}";
            var data = await _cacheService.GetAsync<InviteMemberRequest>(cacheKey, token);

            if (data is null)
                throw CustomExceptionFactory.
                   CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Token is invalid or expired"));

            var result = await _companyMemberRepository.JoinMemberToCompany(data.InviteeMemberId, data.CompanyId, token);
            return _mapper.Map<CompanyMemberResponse>(result);
        }
    }
}
