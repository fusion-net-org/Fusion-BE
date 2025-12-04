

namespace Fusion.Service.ViewModels.Companies.Responses
{
    public class CompanyOfUserResponse
    {
        public Guid Id{ get; set; }
        public string Name { get; set; }
        public string TaxCode { get; set; }
        public DateTime JoinAt { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? Email { get; set; }
        public string? ImageCompany { get; set; }
        public string? AvatarCompany { get; set; }

    }
    public class CompanyOfOwnerResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TaxCode { get; set; }
        public DateTime CreateAt { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? Email { get; set; }
        public string? Detail { get; set; }
        public string? ImageCompany { get; set; }
        public string? AvatarCompany { get; set; }
    }
}
