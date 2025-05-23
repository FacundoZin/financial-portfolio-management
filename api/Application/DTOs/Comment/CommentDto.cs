using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Application.DTOs.Comment
{
    public class CommentDto
    {
        public int ID { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public string Createdby { get; set; } = string.Empty;

        public int? StockID { get; set; }
    }
}