using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Companies.Responses
{
    public class CompanyResponse
    {
        public string? Name { get; set; }

        public Guid? OwnerUserId { get; set; }

        public string? OwnerUserName { get; set; }

        public string? TaxCode { get; set; }

        public string? Detail { get; set; }

        public string? ImageCompany { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime UpdateAt { get; set; }
    }
}
