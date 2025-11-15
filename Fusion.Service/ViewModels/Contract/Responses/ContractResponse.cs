using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Contract.Responses
{
    public class ContractResponse
    {
        public Guid Id { get; set; }
        public Guid ProjectRequestId { get; set; }
        public string ContractCode { get; set; } = string.Empty;
        public string ContractName { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public string Status { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public DateOnly ExpiredDate { get; set; }

        public List<ContractAppendixResponse> Appendices { get; set; } = new();
        public string? Attachment { get; set; }
    }

    public class ContractAppendixResponse
    {
        public Guid Id { get; set; }
        public string AppendixName { get; set; } = string.Empty;
        public string AppendixCode { get; set; } = string.Empty;
    }
}
