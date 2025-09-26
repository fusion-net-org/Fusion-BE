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
        private readonly ICompanyRepository _companyRepository;
        public CompanyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<Guid?> GetCompanyIdByUserId(Guid userId)
        {
            return await _companyRepository.GetCompanyIdByUserId(userId);
        }

        public async Task<string> GetCompanyNameByGuid(Guid company)
        {
            return await _companyRepository.GetCompanyNameByGuid(company);
        }

        public async Task<string> GetMailCompanyByGuid(Guid company)
        {
            return await _companyRepository.GetMailCompanyByGuid(company);
        }

    }
}
