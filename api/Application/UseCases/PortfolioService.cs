using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Common;
using api.Application.DTOs.Portfolio;
using api.Application.Interfaces.External;
using api.Application.Interfaces.Reposiories;
using api.Application.Interfaces.Services;
using api.Domain.Entities;
using api.Infrastructure.Persistence.Repository;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;

namespace api.Application.UseCases
{

    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolioRepository _PortfolioRepo;
        private readonly IaccountService _AccountService;
        private readonly IStockRepository _StockRepo;
        private readonly IFMPService _FMPservice;
        private readonly IHoldingRepository _HoldingRepository;

        public PortfolioService(IPortfolioRepository portfolioRepository, IaccountService accountservice,
            IStockRepository stockRepository, IFMPService fMPService, HoldingRepository holdingRepository)
        {
            _PortfolioRepo = portfolioRepository;
            _AccountService = accountservice;
            _StockRepo = stockRepository;
            _FMPservice = fMPService;
            _HoldingRepository = holdingRepository;
        }

        public async Task<Result<Portfolio>> AddStock(string username, string symbol, int IdPortfolio)
        {
            try
            {
                var usertask = _AccountService.FindByname(username);
                var stocktask = _StockRepo.GetbySymbolAsync(symbol);

                await Task.WhenAll(usertask, stocktask);

                var user = await usertask;
                var stock = await stocktask;

                if (stock == null)
                {
                    var search = await _FMPservice.FindBySymbolAsync(symbol);
                    if (search.Exit == false) return Result<Portfolio>.Error("stock not found", 404);

                    stock = await _StockRepo.Createasync(search.Data);
                }

                if (user == null) return Result<Portfolio>.Error("user not found", 404);

                await AddOrUpdateHolding(user, stock, IdPortfolio);

                return Result<Portfolio>.Exito(null);
            }
            catch (Exception ex)
            {
                return Result<Portfolio>.Error("inteernal server error", 500);
            }
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

        public async Task<Result<Portfolio>> DeleteStock(string username, string symbol, int IdPortfolio)
        {
            var usertask = _AccountService.FindByname(username);
            var stocktask = _StockRepo.GetbySymbolAsync(symbol);

            await Task.WhenAll(usertask, stocktask);

            var user = await usertask;
            var stock = await stocktask;

            if (user == null) return Result<Portfolio>.Error("user not found", 404);
            if (stock == null) return Result<Portfolio>.Error("stock not found", 404);

            var result = await _PortfolioRepo.DeleteStock(new Holding { StockID = stock.ID, AppUserID = user.Id, PortfolioID = IdPortfolio });

            if (result == false) return Result<Portfolio>.Error("something went wrognt", 500);

            return Result<Portfolio>.Exito(null);
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


        private async Task AddOrUpdateHolding(AppUser user, Stock Stock, int PortfolioID)
        {
            if (!user.Holdings.Any(H => H.StockID == Stock.ID))
            {
                Holding holding = new Holding
                {
                    StockID = Stock.ID,
                    AppUserID = user.Id,
                    PortfolioID = PortfolioID
                };

                user.Holdings.Add(holding);

                await _HoldingRepository.AddStockToHolding(user, Stock);
                await _PortfolioRepo.AddStock(holding);


            }
            else
            {
                Holding updated_holding = new Holding
                {
                    StockID = Stock.ID,
                    AppUserID = user.Id,
                    PortfolioID = PortfolioID
                };

                await _HoldingRepository.addrelationship_withportfolio(updated_holding);
                await _PortfolioRepo.AddStock(updated_holding);
            }
        }
    }
}