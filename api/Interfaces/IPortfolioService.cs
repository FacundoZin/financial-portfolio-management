using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Common;
using api.Models;

namespace api.Interfaces
{
    public interface IPortfolioService
    {
        Task<Result<List<Stock>>> GetUserPortfolio(AppUser User);
        Task<Result<List<Stock>>> AddStockToPortfolio(AppUser User, string symbol);
        Task<Result<List<Stock>>> DeleteStock(AppUser user, string symbol);
        Task<bool> ContainStock(string symbol, AppUser User);
    }
}