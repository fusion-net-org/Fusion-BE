using Fusion.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.Contract
{
    public class CreateAppendixRequest
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
    }
    public class UpdateAppendixRequest
    {
        public Guid? Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class ContractSearchRequest : PagedRequest
    {
        public string KeyWord { get; set; } = string.Empty;

        public Range<decimal>? BudgetRange { get; set; }

        public string? CompanyName { get; set; }

        public ContractEnum? Status { get; set; }

        public DateRange<DateOnly>? DateRange { get; set; }

        public ContractDateEnum? StatusDate { get; set; }
    }
}
