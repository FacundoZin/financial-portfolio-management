using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Interfaces.Reposiories;
using api.Infrastructure.Persistence.Data;

namespace api.Infrastructure.Persistence.Repository
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly ApplicationDBcontext _Context;

        public PortfolioRepository(ApplicationDBcontext context)
        {
            _Context = context;
        }

    }
}