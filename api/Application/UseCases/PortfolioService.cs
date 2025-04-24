using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Common;
using api.Application.DTOs.Portfolio;
using api.Application.Interfaces.Reposiories;
using api.Application.Interfaces.Services;
using api.Domain.Entities;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;

namespace api.Application.UseCases
{

    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolioRepository _PortfolioRepo;
        private readonly IaccountService _AccountService;

        public PortfolioService(IPortfolioRepository portfolioRepository, IaccountService accountservice)
        {
            _PortfolioRepo = portfolioRepository;
            _AccountService = accountservice;
        }

        public async Task<Result<PortfolioAddedToUser>> CreatePortfolio(string username, string namePortfolio)
        {
            if (string.IsNullOrEmpty(namePortfolio)) return Result<PortfolioAddedToUser>.Error("please enter the name portfolio", 400);

            var user = await _AccountService.FindByname(username);

            if (user == null) return Result<PortfolioAddedToUser>.Error("sorry we couldn't get the username ", 404);

            var created_portfolio = await _PortfolioRepo.AddPortfolioToUser(username, namePortfolio);

            if (created_portfolio == null) return Result<PortfolioAddedToUser>.Error("something went wrognt", 500);

            PortfolioAddedToUser createdPortfolio = new PortfolioAddedToUser
            {
                IdPortfolio = created_portfolio.Id,
                NamePortfolio = created_portfolio.NamePortfolio
            };

            return Result<PortfolioAddedToUser>.Exito(createdPortfolio);
        }

        public async Task<Result<List<Portfolio>>> GetALL(string username)
        {
            var User = await _AccountService.FindByname(username);

            if (User == null) return Result<List<Portfolio>>.Error("sorry we couldn't get the username ", 404);

            var Portfolios = await _PortfolioRepo.GetAllPortfolios(User.Id);

            return Result<List<Portfolio>>.Exito(Portfolios);
        }

        public async Task<Result<Portfolio>> GetPortfolioByID(string username, int id)
        {
            var user = await _AccountService.FindByname(username);

            if (user == null) return Result<Portfolio>.Error("sorry we couldn't get the username ", 404);

            var portfolio = await _PortfolioRepo.GetPortfolio(user.Id, id);

            if (portfolio == null) return Result<Portfolio>.Error("something went wrognt", 500);

            return Result<Portfolio>.Exito(portfolio);
        }
    }
}