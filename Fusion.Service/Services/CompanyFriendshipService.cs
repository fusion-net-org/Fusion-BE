using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Users.Requests;
using Fusion.Service.ViewModels.Users.Responses;

namespace Fusion.Service.Services
{
    public class CompanyFriendshipService : ICompanyFriendshipService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompanyFriendshipRepository _companyFriendshipRepository;
        private readonly IMapper _mapper;
        private readonly IMailService _mailService;
        private readonly ICompanyRepository _companyRepository;
        public CompanyFriendshipService(IUnitOfWork unitOfWork, ICompanyFriendshipRepository userRepository,
    IMapper mapper, IMailService mailService, ICompanyRepository companyRepository)
        {
            _unitOfWork = unitOfWork;
            _companyFriendshipRepository = userRepository;
            _mapper = mapper;
            _mailService = mailService;
            _companyRepository = companyRepository;
        }

        public async Task<CompanyFriendshipResponse> InviteCompanyFriendship(Guid companyAId, Guid companyBId, Guid requesterId)
        {
            var entity = await _companyFriendshipRepository.InviteCompanyFriendship(
               companyAId,
               companyBId,
               requesterId);

            var nameCompanyA = await _companyRepository.GetCompanyNameByGuid(companyAId);

            var emailCompanyB = await _companyRepository.GetMailCompanyByGuid(companyBId);

            await _unitOfWork.SaveChangesAsync();

            // Gửi mail cho công ty B
            await _mailService.SendEmailAsync(new MailRequest
            {
                ToEmail = emailCompanyB,
                Subject = "Yêu cầu kết bạn từ công ty khác",
                Body = $"Công ty {nameCompanyA} đã gửi lời mời hợp tác. Vui lòng vào chọn Approve/Reject."
            });

            return _mapper.Map<CompanyFriendshipResponse>(entity);
        }

    }
}
