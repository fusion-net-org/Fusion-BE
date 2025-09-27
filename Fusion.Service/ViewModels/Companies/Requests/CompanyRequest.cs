using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Companies.Requests
{
    public class CompanyRequest
    {
        public string? Name { get; set; }
        public string? TaxCode { get; set; }
        public string? Detail { get; set; }
        public IFormFile? ImageCompany { get; set; }
    }
}
