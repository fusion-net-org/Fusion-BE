using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Partner;
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

        public async Task<CompanyFriendshipResponse> AcceptCompanyFriendship(long id, Guid currentUserId)
        {
            var entity = await _companyFriendshipRepository.AcceptCompanyFriendship(id,currentUserId);

            var companyA = await _companyRepository.GetCompanyByIdAsync((Guid)entity.CompanyAId!);
            var companyB = await _companyRepository.GetCompanyByIdAsync((Guid)entity.CompanyBId!);

            await _mailService.SendEmailAsync(new MailRequest
            {
                ToEmail = companyA.Email,
                Subject = "Kết nối đối tác thành công",
                Body = $@"
        <p>Xin chúc mừng!</p>
        <p>Công ty <b>{companyA.Name}</b> và <b>{companyB.Name}</b> đã kết nối thành công như đối tác.</p>
        <p>Hãy cùng bắt đầu hợp tác hiệu quả!</p>"
            });

            await _mailService.SendEmailAsync(new MailRequest
            {
                ToEmail = companyB.Email,
                Subject = "Kết nối đối tác thành công",
                Body = $@"
        <p>Xin chúc mừng!</p>
        <p>Công ty <b>{companyA.Name}</b> và <b>{companyB.Name}</b> đã kết nối thành công như đối tác.</p>
        <p>Chúc mừng sự hợp tác giữa hai bên!</p>"
            });

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CompanyFriendshipResponse>(entity);
        }


        public async Task<CompanyFriendshipResponse> CancelCompanyFriendship(long id, Guid currentUserId)
        {
            var entity = await _companyFriendshipRepository.CancelCompanyFriendship(id,currentUserId);

            var companyA = await _companyRepository.GetCompanyByIdAsync((Guid)entity.CompanyAId!);
            var companyB = await _companyRepository.GetCompanyByIdAsync((Guid)entity.CompanyBId!);

            await _mailService.SendEmailAsync(new MailRequest
            {
                ToEmail = companyA.Email,
                Subject = "Lời mời hợp tác bị từ chối",
                Body = $@"
        <p>Xin thông báo, lời mời hợp tác từ công ty <b>{companyA.Name}</b> tới <b>{companyB!.Name}</b> đã bị từ chối.</p>
        <p>Vui lòng liên hệ lại nếu cần thảo luận thêm.</p>"
            });

            await _mailService.SendEmailAsync(new MailRequest
            {
                ToEmail = companyB.Email,
                Subject = "Lời mời hợp tác bị từ chối",
                Body = $@"
        <p>Xin thông báo, bạn đã từ chối lời mời kết nối từ công ty <b>{companyA.Name}</b>.</p>
        <p>Nếu có sự thay đổi, vui lòng gửi lại yêu cầu hợp tác mới.</p>"
            });

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CompanyFriendshipResponse>(entity);
        }


        public async Task<List<CompanyFriendshipResponse>> GetCompanyFriendshipByCompanyID(Guid userID, Guid companyID)
        {
            var friendships = await _companyFriendshipRepository.GetCompanyFriendshipByCompanyID(userID, companyID);
            var result = _mapper.Map<List<CompanyFriendshipResponse>>(friendships);
            return result;
        }


        public async Task<PagedResult<CompanyFriendshipResponse>> GetCompanyFriendshipByOwnerUserID(Guid ownerUserID, CompanyFriendshipSearchRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _companyFriendshipRepository.GetCompanyFriendshipByOwnerUserID(ownerUserID, request, cancellationToken);

            var mappedItems = _mapper.Map<List<CompanyFriendshipResponse>>(result.Items);

            return new PagedResult<CompanyFriendshipResponse>
            {
                Items = mappedItems,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        public async Task<PagedResult<CompanyFriendshipResponse>> GetCompanyFriendshipByStatus(Guid ownerUserID, string status, PagedRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _companyFriendshipRepository.GetCompanyFriendshipByStatus(ownerUserID,status,request,cancellationToken);

            var mappedItems = _mapper.Map<List<CompanyFriendshipResponse>>(result.Items);

            return new PagedResult<CompanyFriendshipResponse>
            {
                Items = mappedItems,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        public async Task<object> GetCompanyFriendshipStatusSummary(Guid ownerUserId)
        {
            return await _companyFriendshipRepository.GetCompanyFriendshipStatusSummary(ownerUserId);
        }

        public async Task<CompanyFriendshipResponse> InviteCompanyFriendship(Guid companyAId, Guid companyBId, Guid requesterId, string? note)
        {

            var entity = await _companyFriendshipRepository.InviteCompanyFriendship(
               companyAId,
               companyBId,
               requesterId,
               note);

            var nameCompanyA = await _companyRepository.GetCompanyNameByGuid(companyAId);
            var nameCompanyB = await _companyRepository.GetCompanyNameByGuid(companyBId);

            var emailCompanyB = await _companyRepository.GetMailCompanyByGuid(companyBId);
            var emailCompanyA = await _companyRepository.GetMailCompanyByGuid(companyAId);

            await _unitOfWork.SaveChangesAsync();

            // Send notification company A 
            await _mailService.SendEmailAsync(new MailRequest
            {
                ToEmail = emailCompanyA,
                Subject = $"Đã gửi lời mời hợp tác đến công ty {nameCompanyB}",
                Body = $@"
        <p>Xin chào <b>{nameCompanyA}</b>,</p>
        <p>Bạn đã gửi lời mời hợp tác đến công ty <b>{nameCompanyB}</b>.</p>
        <p>Hệ thống sẽ thông báo cho bạn ngay khi công ty <b>{nameCompanyB}</b> phản hồi lời mời này.</p>
        <br/>
        <p><i>Trân trọng,</i><br/>Hệ thống Fusion</p>"
            });

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
