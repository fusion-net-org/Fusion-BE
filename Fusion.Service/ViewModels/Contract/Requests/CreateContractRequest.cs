using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Page.Contract;
using Microsoft.AspNetCore.Http;

namespace Fusion.Service.ViewModels.Contract.Requests
{
    public class CreateContractRequest
    {
        public string ContractCode { get; set; } = string.Empty;
        public string ContractName { get; set; } = string.Empty;

        public DateOnly EffectiveDate { get; set; }
        public DateOnly ExpiredDate { get; set; }

        public decimal Budget { get; set; }

        public List<CreateAppendixRequest> Appendices { get; set; } = new();

        //public IFormFile? AttachmentFile { get; set; }
    }
}
