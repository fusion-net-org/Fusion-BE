using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Contract.Requests
{
    public class UpdateContractRequest
    {
        public string ContractCode { get; set; } = string.Empty;
        public string ContractName { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public DateOnly ExpiredDate { get; set; }
        public List<string> Appendices { get; set; } = new();
    }
}
