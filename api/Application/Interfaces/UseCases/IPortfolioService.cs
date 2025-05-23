using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Common;
using api.Application.DTOs.Portfolio;
using api.Domain.Entities;

namespace api.Application.Interfaces.UseCases
{
    public interface IPortfolioService
    {
        Task<Result<PortfolioAddedToUser>> CreatePortfolio(string username, string namePortfolio);
        Task<Result<Portfolio>> GetPortfolioByID(string username, int id);
        Task<Result<List<Portfolio>>> GetALL(string username);
        Task<Result<Portfolio>> AddStock(string username, string symbol, int IdPortfolio);
        Task<Result<Portfolio>> DeleteStock(string username, string symbol, int IdPortfolio);

    }
}