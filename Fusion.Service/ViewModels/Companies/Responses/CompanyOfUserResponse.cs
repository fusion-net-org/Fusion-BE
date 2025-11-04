

namespace Fusion.Service.ViewModels.Companies.Responses
{
    public class CompanyOfUserResponse
    {
        public Guid Id{ get; set; }
        public string Name { get; set; }
        public string TaxCode { get; set; }
        public DateTime JoinAt { get; set; }
    }
    public class CompanyOfOwnerResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TaxCode { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
