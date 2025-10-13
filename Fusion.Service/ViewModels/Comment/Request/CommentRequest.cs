using System;
using System.Collections.Generic;


namespace Fusion.Service.ViewModels.Comment.Request
{
    public class CommentRequest
    {
        public Guid? TaskId { get; set; }

        public string? Body { get; set; }

    }
    public class CommentRequestUpdate
    {
        public long? Id;
        public string? Body { get; set; }

    }
}
