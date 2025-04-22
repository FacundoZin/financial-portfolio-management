using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Interfaces.Reposiories;
using api.Application.Interfaces.Services;

namespace api.Application.UseCases
{

    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolioRepository _PortfolioRepo;

        public PortfolioService(IPortfolioRepository portfolioRepository)
        {
            _PortfolioRepo = portfolioRepository;
        }

    }
}