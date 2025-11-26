using Microsoft.AspNetCore.Http;

namespace Fusion.Service.ViewModels.Companies.Requests
{
    public class CompanyRequest
    {
        public string? Name { get; set; }
        public string? TaxCode { get; set; }
        public string? Detail { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }     
        public string? Address { get; set; }         
        public string? Website { get; set; }
        public IFormFile? ImageCompany { get; set; }
        public IFormFile? AvatarCompany { get; set; }
    }
}
