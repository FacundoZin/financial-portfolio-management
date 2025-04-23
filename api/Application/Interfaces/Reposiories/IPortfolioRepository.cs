using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Domain.Entities;
using api.Infrastructure.Persistence.Repository;

namespace api.Application.Interfaces.Reposiories
{
    public interface IPortfolioRepository
    {
        Task<Portfolio?> AddPortfolioToUser(string UserID, string namePortfolio);

    }
}