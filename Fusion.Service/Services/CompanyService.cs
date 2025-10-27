using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.Helpers;
using Fusion.Service.IServices;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Companies.Validators;
using Fusion.Service.ViewModels.Users.Responses;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Fusion.Service.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IMapper _mapper;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyMemberRepository _companyMemberRepository;
        private readonly IValidator<CompanyRequest> _validator;
        private readonly ICompanyFriendshipRepository _companyFriendshipRepository;
        private readonly IMailService _mailService;
        private readonly ICompanyActivityService _logService;

        public CompanyService(IMapper mapper, ICompanyRepository companyRepository, ICloudinaryService cloudinaryService
            , IUserRepository userRepository, ICompanyMemberRepository companyMemberRepository, IValidator<CompanyRequest> validator, ICompanyFriendshipRepository companyFriendshipRepository,
            IMailService mailService, ICompanyActivityService logService)
        {
            _mapper = mapper;
            _companyRepository = companyRepository;
            _cloudinaryService = cloudinaryService;
            _userRepository = userRepository;
            _companyMemberRepository = companyMemberRepository;
            _validator = validator;
            _companyFriendshipRepository = companyFriendshipRepository;
            _mailService = mailService;
            _logService = logService;
        }

        public async Task<CompanyResponse> CreateCompanyAsync(CompanyRequest request, string Email, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT);

            //true: bắt buộc phải có image_company
            await _validator.ValidateAndThrowAsync(
                request,
                opts => opts.IncludeRuleSets("Create"),
                cancellationToken
                );

            //check người đăng kí có tồn tại đăng kí hay không
            var user = await _userRepository.GetUserByEmailAsync(Email, cancellationToken);
            if (user == null)
                throw CustomExceptionFactory.
                    CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Email incorrect!"));

            //check tax-code có tồn tại duy nhất hay không (trong hệ thống)
            var company_taxcode_existed = await _companyRepository.GetCompanyByTaxCode(request.TaxCode);
            if (company_taxcode_existed != null)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.EXISTED.FormatMessage("Tax-code"));

            var company_email_existed = await _companyRepository.GetCompanyByEmail(request.Email);
            if (company_email_existed != null)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.EXISTED.FormatMessage("Company Email"));

            var image_company = await _cloudinaryService.UploadImageAsync(request.ImageCompany, "CompanyBanner", cancellationToken);
            var avatar_company = await _cloudinaryService.UploadImageAsync(request.AvatarCompany, "CompanyAvatar", cancellationToken);

            var company = _mapper.Map<Company>(request);

            var newCompany = await _companyRepository.AddCompanyAsync(user, image_company, avatar_company, company, cancellationToken);

            if (newCompany == null)
                throw CustomExceptionFactory.CreateInternalServerError(ResponseMessages.INTERNAL_SERVER_ERROR.FormatMessage("Add Company fail"));

            var newMember = await _companyMemberRepository.AddCompanyMemberAsync(new CompanyMember
            {
                CompanyId = newCompany.Id,
                UserId = user.Id,
                Status = "Active",
                IsDeleted = false,
                JoinedAt = DateTime.UtcNow.AddHours(7),
            }, cancellationToken);

            if (newMember == null)
                throw CustomExceptionFactory.CreateInternalServerError(ResponseMessages.INTERNAL_SERVER_ERROR.FormatMessage("Add new member fail"));


            var emailBody = MailUtils.CreateCompanyThankYouEmail(
                user.UserName, newCompany.Name, "http://localhost:5173/", "http://localhost:5173/company");

            await _mailService.SendEmailAsync(new ViewModels.Companies.Email.MailRequest()
            {
                Subject = $"Welcome {company.Name} to Fusion Platform - Manage, Collaborate, and Grow",
                Body = emailBody,
                ToEmail = user.Email
            });
            return _mapper.Map<CompanyResponse>(newCompany);

        }

        public async Task<PagedResult<CompanyResponse>> GetPagedCompaniesAsync(string userMail, CompanyPagedSearchRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT);

            var result = await _companyRepository.GetPagedCompaniesAsync(userMail, request, cancellationToken);

            if (result == null || result.Items.Count == 0)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Companies"));

            var list = new PagedResult<CompanyResponse>
            {
                Items = _mapper.Map<List<CompanyResponse>>(result.Items),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

            foreach (var item in list.Items)
            {
                var company = result.Items.FirstOrDefault(c => c.Id == item.Id);
                if (company == null) continue;

                var friendships = company.CompanyFriendshipCompanyAs
                    .Concat(company.CompanyFriendshipCompanyBs)
                    .ToList();

                item.TotalApproved = friendships.Count(f => f.Status == "Active");
                item.TotalWaitForApprove = friendships.Count(f => f.Status == "Pending");
                item.TotalPartners = friendships.Count();
            }
            return list;
        }
        public async Task<PagedResult<CompanyResponseVersion2>> GetAllCompaniesAsync(
    string userMail,
    CompanyPagedSearchRequestVersion2 request,
    Guid? selectedCompanyId = null,
    CancellationToken cancellationToken = default)
        {
            var currentUser = await _userRepository.GetUserByEmailAsync(userMail);
            if (currentUser == null)
                throw CustomExceptionFactory.CreateUnauthorizedError("Don't find information user!");

            var currentUserId = currentUser.Id;

            Guid? currentCompanyA = selectedCompanyId ?? await _companyRepository.GetCompanyIdByUserId(currentUserId);

            var partnerCompanyIds = new List<Guid>();

            var pendingPartnerIds = new List<Guid>();

            if (currentCompanyA != null)
            {
                var friendships = await _companyFriendshipRepository.GetCompanyFriendshipByCompanyID(currentUserId, currentCompanyA.Value);

                partnerCompanyIds = friendships
                     .Where(f => f.Status.ToLower() == "active")
                     .Select(f => f.CompanyAId == currentCompanyA ? f.CompanyBId : f.CompanyAId)
                     .Where(id => id.HasValue)
                     .Select(id => id.Value)
                     .Distinct()
                     .ToList();



                pendingPartnerIds = friendships
                    .Where(f => f.Status.ToLower() == "Pending".ToLower())
                    .Select(f => f.CompanyAId == currentCompanyA ? f.CompanyBId : f.CompanyAId)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .Distinct()
                    .ToList();

            }


            var result = await _companyRepository.GetAllCompaniesAsync(userMail, request, selectedCompanyId, cancellationToken);

            if (result == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Companies"));


            var list = new PagedResult<CompanyResponseVersion2>
            {

                Items = result.Items.Select(company =>
                {
                    var item = _mapper.Map<CompanyResponseVersion2>(company);

                    item.isOwner = company.OwnerUserId == currentUserId;
                    item.isPartner = currentCompanyA != null && partnerCompanyIds.Contains(company.Id);
                    item.isPendingAprovePartner = currentCompanyA != null && pendingPartnerIds.Contains(company.Id);

                    return item;
                }).ToList(),

                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

            foreach (var item in list.Items)
            {
                var company = result.Items.FirstOrDefault(c => c.Id == item.Id);
                if (company == null) continue;

                var friendships = company.CompanyFriendshipCompanyAs
                    .Concat(company.CompanyFriendshipCompanyBs)
                    .ToList();

                item.TotalApproved = friendships.Count(f => f.Status == "Active");
                item.TotalWaitForApprove = friendships.Count(f => f.Status == "Pending");
                item.TotalPartners = friendships.Count();
            }

            return list;
        }

        public async Task<Guid?> GetCompanyIdByUserId(Guid userId)
        {
            return await _companyRepository.GetCompanyIdByUserId(userId);
        }

        public async Task<CompanyResponse> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken = default)
        {
            var company = await _companyRepository.GetCompanyByIdAsync(companyId);


            var friendships = company.CompanyFriendshipCompanyAs
                .Concat(company.CompanyFriendshipCompanyBs)
                .ToList();
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company"));

            var result = _mapper.Map<CompanyResponse>(company);

            result.TotalApproved = friendships.Count(f => f.Status == "Active");
            result.TotalWaitForApprove = friendships.Count(f => f.Status == "Pending");
            result.TotalPartners = friendships.Count();

            return result;
        }

        public async Task<string> GetCompanyNameByGuid(Guid company)
        {
            return await _companyRepository.GetCompanyNameByGuid(company);
        }

        public async Task<CompanyResponse> UpdateCompanyAsync(Guid companyId, CompanyRequest request, string Email, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

            // false: không bắt buộc ImageCompany
            await _validator.ValidateAndThrowAsync(
                request,
                opts => opts.IncludeRuleSets("Update"),
                cancellationToken);

            #region Validate Business Company Update
            var user = await _userRepository.GetUserByEmailAsync(Email, cancellationToken);
            if (user == null)
                throw CustomExceptionFactory.
                    CreateBadRequestError(ResponseMessages.NOT_FOUND.FormatMessage("Email!"));

            var company = await _companyRepository.GetCompanyByIdAsync(companyId);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company"));

            if (company.OwnerUserId != user.Id)
                throw CustomExceptionFactory
                    .CreateBadRequestError(ResponseMessages.BAD_REQUEST,$"Company is not belong to {company.OwnerUser.UserName}");

            if (!string.IsNullOrEmpty(request.Email) && request.Email != company.Email)
            {
                var company_email_existed = await _companyRepository.GetCompanyByEmail(request.Email);
                if (company_email_existed != null)
                    throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.EXISTED.FormatMessage("Company Email"));
            }

            if (!string.IsNullOrEmpty(request.TaxCode) && request.TaxCode != company.TaxCode)
            {
                var company_taxcode_existed = await _companyRepository.GetCompanyByTaxCode(request.TaxCode);
                if (company_taxcode_existed != null)
                    throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.DUPLICATE.FormatMessage("Tax-code"));
            }

            var image_company = "";
            var avatar_company = "";

            if (request.ImageCompany != null && request.ImageCompany.Length > 0)
            {
                await _cloudinaryService.DeleteImageAsync(_cloudinaryService.ExtractPublicIdFromUrl(company.ImageCompany), cancellationToken);
                image_company = await _cloudinaryService.UploadImageAsync(request.ImageCompany, "CompanyBanner", cancellationToken);
            }
            else
            {
                image_company = company.ImageCompany;
            }

            if (request.AvatarCompany != null && request.AvatarCompany.Length > 0)
            {
                await _cloudinaryService.DeleteImageAsync(_cloudinaryService.ExtractPublicIdFromUrl(company.AvatarCompany), cancellationToken);
                avatar_company = await _cloudinaryService.UploadImageAsync(request.AvatarCompany, "CompanyAvatar", cancellationToken);
            }
            else
            {
                avatar_company = company.AvatarCompany;
            }
            #endregion

            var result = await _companyRepository.UpdateCompanyAsync(image_company, avatar_company, companyId, _mapper.Map<Company>(request), cancellationToken);

            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = user.Id,
                Title = "Update Company Information",
                Description = $"Company '{company.Name}' information has been updated by user id:'{user.UserName}'.",

            };
            await _logService.CreateLog(log, cancellationToken);
            return _mapper.Map<CompanyResponse>(result);
        }

        public async Task<string> GetMailCompanyByGuid(Guid company)
        {
            return await _companyRepository.GetMailCompanyByGuid(company);
        }

        public async Task<bool> DeleteCompanyAsync(Guid companyId, string Email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetUserByEmailAsync(Email, cancellationToken);
            if (user == null)
                throw CustomExceptionFactory.
                    CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Email incorrect!"));

            var company = await _companyRepository.GetCompanyByIdAsync(companyId);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company"));

            if (company.OwnerUserId != user.Id)
                throw CustomExceptionFactory
                    .CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Owner User in this company"));

            await _companyRepository.DeleteCompanyAsync(company, cancellationToken);

            var log = new CompanyActivityLog
            {
                CompanyId = companyId,
                ActorUserId = user.Id,
                Title = "Deleted Company",
                Description = $"Company '{company.Name}' has been deleted by user id:'{user.UserName}'.",

            };
            await _logService.CreateLog(log, cancellationToken);
            return true;
        }


    }
}
