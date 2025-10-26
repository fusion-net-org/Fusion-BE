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
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Fusion.Service.Services
{
    public class CompanyFriendshipService : ICompanyFriendshipService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompanyFriendshipRepository _companyFriendshipRepository;
        private readonly IMapper _mapper;
        private readonly IMailService _mailService;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICompanyActivityService _logService;
        public CompanyFriendshipService(IUnitOfWork unitOfWork, ICompanyFriendshipRepository userRepository,
    IMapper mapper, IMailService mailService, ICompanyRepository companyRepository, ICompanyActivityService logService)
        {
            _unitOfWork = unitOfWork;
            _companyFriendshipRepository = userRepository;
            _mapper = mapper;
            _mailService = mailService;
            _companyRepository = companyRepository;
            _logService = logService;
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

        public async Task<PagedResult<CompanyFriendshipResponse>> GetCompanyFriendshipByCompanyIDVersion2(
              Guid userID,
              Guid companyID,
              CompanyFriendshipSearchRequest request,
              CancellationToken cancellationToken = default)
        {
            var pagedFriendships = await _companyFriendshipRepository
                .GetCompanyFriendshipByCompanyIDVersion2(userID, companyID, request, cancellationToken);

            var mappedItems = _mapper.Map<List<CompanyFriendshipResponse>>(pagedFriendships.Items);

            return new PagedResult<CompanyFriendshipResponse>
            {
                Items = mappedItems,
                TotalCount = pagedFriendships.TotalCount,
                PageNumber = pagedFriendships.PageNumber,
                PageSize = pagedFriendships.PageSize
            };
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

        public async Task<PagedResult<CompanyFriendshipResponse>> GetCompanyFriendshipByStatus(Guid ownerUserID, Guid companyID, string status, PagedRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _companyFriendshipRepository.GetCompanyFriendshipByStatus(ownerUserID, companyID, status,request,cancellationToken);

            var mappedItems = _mapper.Map<List<CompanyFriendshipResponse>>(result.Items);

            return new PagedResult<CompanyFriendshipResponse>
            {
                Items = mappedItems,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        public async Task<object> GetCompanyFriendshipStatusSummary(Guid ownerUserId, Guid? companyId = null)
        {
            return await _companyFriendshipRepository.GetCompanyFriendshipStatusSummary(ownerUserId, companyId);
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
            var log = new CompanyActivityLog
            {
                CompanyId = companyAId,
                ActorUserId = requesterId,
                Title = "Invite Company Friendship",
                Description = $"User id:'{requesterId}' sent an invitation to partner '{companyBId}'.",

            };
            await _logService.CreateLog(log);
            return _mapper.Map<CompanyFriendshipResponse>(entity);
        }
        public async Task<CompanyFriendshipResponse?> GetCompanyFriendshipBetweenCompaniesAsync(
            Guid companyAId,
            Guid companyBId,
            CancellationToken token = default)
        {
            var entity = await _companyFriendshipRepository.GetCompanyFriendshipBetweenCompaniesAsync(companyAId, companyBId, token);

            if (entity == null)
                return null;

            var response = _mapper.Map<CompanyFriendshipResponse>(entity);

            var totalProjectsA = entity.CompanyA?.ProjectCompanies?.Count ?? 0;
            var totalProjectsB = entity.CompanyB?.ProjectCompanies?.Count ?? 0;
            var totalMembersA = entity.CompanyA?.CompanyMembers?.Count ?? 0;
            var totalMembersB = entity.CompanyB?.CompanyMembers?.Count ?? 0;

            response.TotalProject = totalProjectsA + totalProjectsB;
            response.TotalMember = totalMembersA + totalMembersB;

            return response;
        }

        public async Task<CompanyFriendshipResponse> DeleteCompanyFriendship(long id, Guid currentUserId)
        {
            var entity = await _companyFriendshipRepository.DeleteCompanyFriendship(id, currentUserId);

            var companyA = await _companyRepository.GetCompanyByIdAsync((Guid)entity.CompanyAId!);
            var companyB = await _companyRepository.GetCompanyByIdAsync((Guid)entity.CompanyBId!);

            await _mailService.SendEmailAsync(new MailRequest
            {
                ToEmail = companyA.Email,
                Subject = "Đối tác đã hủy kết nối",
                Body = $@"
        <p>Xin thông báo, công ty <b>{companyB.Name}</b> đã hủy kết nối đối tác với công ty <b>{companyA.Name}</b>.</p>
        <p>Nếu đây là nhầm lẫn, bạn có thể gửi lại lời mời hợp tác mới.</p>"
            });

            await _mailService.SendEmailAsync(new MailRequest
            {
                ToEmail = companyB.Email,
                Subject = "Đã hủy kết nối đối tác",
                Body = $@"
        <p>Bạn vừa hủy kết nối đối tác với công ty <b>{companyA.Name}</b>.</p>
        <p>Trạng thái mối quan hệ hiện là <b>Inactive</b>.</p>"
            });

            await _unitOfWork.SaveChangesAsync();

            var log = new CompanyActivityLog
            {
                CompanyId = companyB.Id,
                ActorUserId = currentUserId,
                Title = "Delete Company Friendship",
                Description = $"User id:'{currentUserId}'  has cancelled the invitation to become a partner of the company with id:'{companyA.Id}'.",

            };
            await _logService.CreateLog(log);
            return _mapper.Map<CompanyFriendshipResponse>(entity);
        }

        /*****************************************************************************************Mobile*****************************************************************/
        public async Task<PagedResult<PartnerResponse>> GetCompanyFriendshipByCompanyID(Guid ownerUserID, Guid companyID, CompanyFriendshipSearchRequest request, CancellationToken token)
        {
            var result = await _companyFriendshipRepository.GetCompanyFriendshipByCompanyID(ownerUserID, companyID, request, token);

            var partners = new List<PartnerResponse>();
            var addedCompanyIds = new HashSet<Guid>();

            foreach (var friendship in result.Items)
            {
                var partnerCompany = friendship.CompanyAId == companyID ? friendship.CompanyB: friendship.CompanyA;

                if (partnerCompany != null && partnerCompany.Id != companyID && addedCompanyIds.Add(partnerCompany.Id))
                {
                    partners.Add(new PartnerResponse
                    {
                        CompanyId = partnerCompany.Id,
                        Name = partnerCompany.Name,
                        OwnerUserName = partnerCompany.OwnerUser?.UserName,
                        TaxCode = partnerCompany.TaxCode,
                        RespondedAt = friendship.RespondedAt,
                        CreatedAt = friendship.CreatedAt,
                        TotalProject = partnerCompany.ProjectCompanies.Count + partnerCompany.ProjectCompanyHireds.Count,
                    });
                }
            }

            return new PagedResult<PartnerResponse>
            {
                Items = partners,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        public async Task<object> GetCompanyFriendshipStatusSummary(Guid ownerUserId, Guid companyId)
        {
            return await _companyFriendshipRepository.GetCompanyFriendshipStatusSummary(ownerUserId, companyId);
        }

      
    }
}
