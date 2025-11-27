using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.UserLog.Responses
{
    public class UserLogResponse
    {
        public Guid Id { get; set; }

        public Guid? ActorUserId { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateAt { get; set; }

        public bool IsDeleted { get; set; }
    }
}
