using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Email;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Users.Requests;
using Microsoft.AspNetCore.Http;

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

        public async Task<CompanyFriendshipResponse> AcceptCompanyFriendship(long id)
        {
            var entity = await _companyFriendshipRepository.AcceptCompanyFriendship(id);
            return _mapper.Map<CompanyFriendshipResponse>(entity);
        }

        public async Task<CompanyFriendshipResponse> CancelCompanyFriendship(long id)
        {
            var entity = await _companyFriendshipRepository.CancelCompanyFriendship(id);
            return _mapper.Map<CompanyFriendshipResponse>(entity);
        }

        public async Task<List<CompanyFriendship>> GetCompanyFriendshipByOwnerUserID(Guid ownerUserID)
        {
            return await _companyFriendshipRepository.GetCompanyFriendshipByOwnerUserID(ownerUserID);
        }

        public async Task<List<CompanyFriendship>> GetCompanyFriendshipByStatus(string status)
        {
            return await _companyFriendshipRepository.GetCompanyFriendshipByStatus(status);
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
                Body = $@"
        <p>Công ty <b>{nameCompanyA}</b> đã gửi lời mời hợp tác.</p>
        <p>Vui lòng chọn một hành động:</p>
        <a href='https://localhost:7160/api/partners/accept/{entity.Id}' 
           style='background-color:green;color:white;padding:10px 15px;text-decoration:none;border-radius:5px;margin-right:10px;'>Approve</a>
        <a href='https://localhost:7160/api/partners/cancel/{entity.Id}' 
           style='background-color:red;color:white;padding:10px 15px;text-decoration:none;border-radius:5px;'>Reject</a>
         ",
            });

            return _mapper.Map<CompanyFriendshipResponse>(entity);
        }

    }
}
