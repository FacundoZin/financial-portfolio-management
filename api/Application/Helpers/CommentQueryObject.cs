using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Application.Helpers
{
    public class CommentQueryObject
    {
        public string Symbol { get; set; } = string.Empty;
        public bool IsDecsending { get; set; }
    }
}