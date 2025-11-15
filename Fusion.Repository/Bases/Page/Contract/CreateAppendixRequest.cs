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

}
