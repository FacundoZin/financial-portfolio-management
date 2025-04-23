using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Application.DTOs.Portfolio
{
    public class PortfolioAddedToUser
    {
        public int IdPortfolio { get; set; }
        public string NamePortfolio { get; set; } = string.Empty;
    }
}