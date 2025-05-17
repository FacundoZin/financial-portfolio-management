using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Domain.Entities;
using api.Infrastructure.Persistence.Repository;

namespace api.Application.Interfaces.Infrastructure.Reposiories
{
    public interface IPortfolioRepository
    {
        Task<Portfolio?> AddPortfolioToUser(string UserID, string namePortfolio);
        Task<Portfolio?> GetPortfolio(string UserID, int IdPortfolio);
        Task<List<Portfolio>?> GetAllPortfolios(string UserID);
        Task<bool> AddStock(Holding holding);
        Task<bool> DeleteStock(Holding holding);

    }
}