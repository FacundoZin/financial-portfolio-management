using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.DTOs.Comment;

namespace api.Application.DTOs.Stock
{
    public class StockDto
    {
        public int ID { get; set; }

        public string Symbol { get; set; } = string.Empty;

        public string Companyname { get; set; } = string.Empty;

        public decimal Purchase { get; set; }

        public decimal LastDiv { get; set; }

        public string Industry { get; set; } = string.Empty;

        public long MarketCap { get; set; }

        public List<CommentDto> comments { get; set; }
    }
}