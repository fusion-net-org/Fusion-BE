using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Enums
{
    public enum ProjectRequestStatusEnum
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
        Finished = 3,
        PendingClosed = 4,
        AcceptedClosed = 5,
        RejectedClosed = 6,
    }

    public enum ProjectRequestViewMode
    {
        AsRequester, // Công ty thuê
        AsExecutor   // Công ty được thuê
    }

    public enum DateFilterType
    {
        CreatedDate, // lấy trên cái Ngày tạo
        StartEndDate, // Lấy trên Start và End và Status 
        ApprovedDate, //Lấy trên UpdateAt và Status Acceptd | Finished
        RejectedDate, // Lấy trên UpdateAt và Status là Reject
        PendingDate //Lấy trên CreateAt và Status Pending
    }
}
