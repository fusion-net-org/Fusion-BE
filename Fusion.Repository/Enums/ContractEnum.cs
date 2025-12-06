using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Enums
{
    public enum ContractEnum
    {
        DRAFT,
        Accepted,
        Rejected,
        Pending
    }

    public enum ContractDateEnum
    {
        EffectiveDate,
        ExpiredDate,
        CreateAt
    }
}
