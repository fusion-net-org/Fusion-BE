using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface ICompanyRepository
    {
        Task<string> GetMailCompanyByGuid(Guid company);
        Task<string> GetCompanyNameByGuid(Guid company);
        Task<Guid?> GetCompanyIdByUserId(Guid userId);
    }
}
