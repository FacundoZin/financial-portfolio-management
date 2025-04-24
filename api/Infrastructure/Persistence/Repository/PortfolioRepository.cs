using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Interfaces.Reposiories;
using api.Domain.Entities;
using api.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace api.Infrastructure.Persistence.Repository
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly ApplicationDBcontext _Context;

        public PortfolioRepository(ApplicationDBcontext context)
        {
            _Context = context;
        }

        public async Task<Portfolio?> AddPortfolioToUser(string UserID, string namePortfolio)
        {
            var portfolio = new Portfolio
            {
                AppUserID = UserID,
                NamePortfolio = namePortfolio
            };

            _Context.portfolios.Add(portfolio);
            var result = await _Context.SaveChangesAsync();

            if (result > 0) return portfolio;

            return null;

        }

        public async Task<List<Portfolio>?> GetAllPortfolios(string UserID)
        {
            var portfolios = await _Context.portfolios.Where(p => p.AppUserID == UserID).ToListAsync();

            return portfolios;
        }

        public async Task<Portfolio?> GetPortfolio(string UserID, int IdPortfolio)
        {
            var portfolio = await _Context.portfolios.FirstOrDefaultAsync(p => p.AppUserID == UserID && p.Id == IdPortfolio);

            if (portfolio == null) return null;

            return portfolio;

        }
    }
}