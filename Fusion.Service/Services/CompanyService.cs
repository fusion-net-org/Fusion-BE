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
using Fusion.Service.IServices;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.Commons.Helpers; // import extension

using Fusion.Service.ViewModels.Companies.Validators;
using Fusion.Service.ViewModels.Users.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IMapper _mapper;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IUserRepository _userRepository;
        private readonly IValidator<CompanyRequest> _validator;

        public CompanyService(IMapper mapper, ICompanyRepository companyRepository, ICloudinaryService cloudinaryService
            , IUserRepository userRepository, IValidator<CompanyRequest> validator)
        {
            _mapper = mapper;
            _companyRepository = companyRepository;
            _cloudinaryService = cloudinaryService;
            _userRepository = userRepository;
            _validator = validator;
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
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.EXISTED.FormatMessage("Tax-code is existed in the system"));

            var company_email_existed = await _companyRepository.GetCompanyByEmail(request.Email);
            if (company_email_existed != null)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.EXISTED.FormatMessage("Company Email is existed in the system"));

            var image_company = await _cloudinaryService.UploadImageAsync(request.ImageCompany, "Company", cancellationToken);

            var company = _mapper.Map<Company>(request);

            var newCompany = await _companyRepository.AddCompanyAsync(user, image_company, company, cancellationToken);
            return _mapper.Map<CompanyResponse>(newCompany);

        }

        public async Task<PagedResult<CompanyResponse>> GetPagedCompaniesAsync(CompanyPagedSearchRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT);

            var result = await _companyRepository.GetPagedCompaniesAsync(request, cancellationToken);

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
            return list;
        }

        public async Task<Guid?> GetCompanyIdByUserId(Guid userId)
        {
            return await _companyRepository.GetCompanyIdByUserId(userId);
        }

        public async Task<CompanyResponse> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken = default)
        {
            var company = await _companyRepository.GetCompanyByIdAsync(companyId);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company"));

            return _mapper.Map<CompanyResponse>(company);
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
                    CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Email incorrect!"));

            var company = await _companyRepository.GetCompanyByIdAsync(companyId);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company"));

            if (company.OwnerUserId != user.Id)
                throw CustomExceptionFactory
                    .CreateNotFoundError(ResponseMessages.BAD_REQUEST.FormatMessage($"Company is not belong to {company.OwnerUser.UserName}"));

            if (!string.IsNullOrEmpty(request.Email) && request.Email != company.Email)
            {
                var company_email_existed = await _companyRepository.GetCompanyByEmail(request.Email);
                if (company_email_existed != null)
                    throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.EXISTED.FormatMessage("Company Email is existed in the system"));
            }

            if (!string.IsNullOrEmpty(request.TaxCode) && request.TaxCode != company.TaxCode)
            {
                var company_taxcode_existed = await _companyRepository.GetCompanyByTaxCode(request.TaxCode);
                if (company_taxcode_existed != null)
                    throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.EXISTED.FormatMessage("Tax-code is existed in the system"));
            }

            var image_company = "";

            if (request.ImageCompany != null && request.ImageCompany.Length > 0)
            {
                await _cloudinaryService.DeleteImageAsync(_cloudinaryService.ExtractPublicIdFromUrl(company.ImageCompany), cancellationToken);
                image_company = await _cloudinaryService.UploadImageAsync(request.ImageCompany, "Company", cancellationToken);
            }
            else
            {
                image_company = company.ImageCompany;
            }
                #endregion

                var result = await _companyRepository.UpdateCompanyAsync(image_company, companyId, _mapper.Map<Company>(request), cancellationToken);

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
            if (company.OwnerUserId != user.Id)
                throw CustomExceptionFactory
                    .CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company"));

            await _companyRepository.DeleteCompanyAsync(company, cancellationToken);
            return true;
        }
    }
}
