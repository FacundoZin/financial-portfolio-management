using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Common;
using api.Application.DTOs.Portfolio;

namespace api.Application.Interfaces.Services
{
    public interface IPortfolioService
    {
        Task<Result<PortfolioAddedToUser>> CreatePortfolio(string username, string namePortfolio);

    }
}