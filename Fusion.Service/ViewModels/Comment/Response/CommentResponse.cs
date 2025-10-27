using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Service.ViewModels.Comment.Response
{
    public class CommentResponse
    {
        public long Id { get; set; }

        public Guid? TaskId { get; set; }

        public Guid? AuthorUserId { get; set; }

        public string? Body { get; set; }

        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }

        public string? Status { get; set; }

    }
}
