using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Users.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.IServices;

namespace Fusion.Service.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        //private readonly ICloudinaryService _cloudinaryService;

        public CompanyService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<CompanyResponse> CreateCompanyAsync(CompanyRequest request, string Email, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT);

            //check người đăng kí có tồn tại đăng kí hay không
            var user = await _unitOfWork.userRepository.GetUserByEmailAsync(Email, cancellationToken);
            if (user == null)
                throw CustomExceptionFactory.
                    CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Email incorrect!"));

            //check tax-code có tồn tại duy nhất hay không (dùng API VietQR chưa implement)


            //check tax-code có tồn tại duy nhất hay không (trong hệ thống)
            var company_existed = await _unitOfWork.companyRepository.FindAsync(c => c.TaxCode == request.TaxCode);
            if (company_existed != null)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.EXISTED.FormatMessage("Tax-code is existed in the system"));

            //var image_company = await _cloudinaryService.UploadImageAsync(request.ImageCompany, "Company", cancellationToken);

            var company = _mapper.Map<Company>(request);
            company.OwnerUserId = user.Id;
            company.ImageCompany = "image_company"; //chưa test image cloud
            company.CreateAt = DateTime.UtcNow.AddHours(7);

            var newCompany = await _unitOfWork.companyRepository.AddAsync(company, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CompanyResponse>(newCompany);

        }

        public async Task<PagedResult<CompanyResponse>> GetPagedCompaniesAsync(CompanyPagedSearchRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.INVALID_INPUT);

            var result = await _unitOfWork.companyRepository.GetPagedCompaniesAsync(request, cancellationToken);

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
            return await _unitOfWork.companyRepository.GetCompanyIdByUserId(userId);
        }

        public async Task<CompanyResponse> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken = default)
        {
            var company = await _unitOfWork.companyRepository.FindAsync(c => c.Id == companyId);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company"));

            return _mapper.Map<CompanyResponse>(company);

        }
        public async Task<string> GetCompanyNameByGuid(Guid company)
        {
            return await _unitOfWork.companyRepository.GetCompanyNameByGuid(company);
        }

        public async Task<CompanyResponse> UpdateCompanyAsync(Guid companyId, CompanyRequest request, string Email, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

            var user = await _unitOfWork.userRepository.GetUserByEmailAsync(Email, cancellationToken);
            if (user == null)
                throw CustomExceptionFactory.
                    CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Email incorrect!"));

            var company = await _unitOfWork.companyRepository.FindAsync(c => c.Id == companyId && c.OwnerUserId == user.Id);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company"));

            company.Name = request.Name ?? company.Name;
            company.TaxCode = request.TaxCode ?? company.TaxCode;
            company.Detail = request.Detail ?? company.Detail;
            company.UpdateAt = DateTime.UtcNow.AddHours(7);
            //company.ImageCompany (chưa test cái cloud nên handle sau)

            _unitOfWork.companyRepository.Update(company);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CompanyResponse>(company);
        }
        public async Task<string> GetMailCompanyByGuid(Guid company)
        {
            return await _unitOfWork.companyRepository.GetMailCompanyByGuid(company);
        }

        public async Task<bool> DeleteCompanyAsync(Guid companyId, string Email, CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.userRepository.GetUserByEmailAsync(Email, cancellationToken);
            if (user == null)
                throw CustomExceptionFactory.
                    CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Email incorrect!"));

            var company = await _unitOfWork.companyRepository.FindAsync(c => c.Id == companyId && user.Id == c.OwnerUserId);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company"));

            _unitOfWork.companyRepository.Remove(company);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
