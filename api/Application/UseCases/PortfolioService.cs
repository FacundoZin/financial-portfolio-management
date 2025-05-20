using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Common;
using api.Application.DTOs.Portfolio;
using api.Application.Interfaces.Infrastructure.BackgrounServices;
using api.Application.Interfaces.Infrastructure.FMP_Client;
using api.Application.Interfaces.Infrastructure.Messaging;
using api.Application.Interfaces.Infrastructure.Reposiories;
using api.Application.Interfaces.UseCases;
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
        private readonly IBackgroundTaskQueue _TaskQueue;
        private readonly IStockFollowPublisher _Publisher;
        private readonly ILogger _Logger;

        public PortfolioService(IPortfolioRepository portfolioRepository, IaccountService accountservice,
            IStockRepository stockRepository, IFMPService fMPService, HoldingRepository holdingRepository,
            IBackgroundTaskQueue taskQueue, IStockFollowPublisher publisher, ILogger logger)
        {
            _PortfolioRepo = portfolioRepository;
            _AccountService = accountservice;
            _StockRepo = stockRepository;
            _FMPservice = fMPService;
            _HoldingRepository = holdingRepository;
            _TaskQueue = taskQueue;
            _Publisher = publisher;
            _Logger = logger;
        }

        public async Task<Result<Portfolio>> AddStock(string username, string symbol, int IdPortfolio)
        {
            try
            {
                var usertask = _AccountService.FindByname(username);
                var stocktask = GetStockAsync(symbol);

                await Task.WhenAll(usertask, stocktask);

                var user = usertask.Result;
                var stock = stocktask.Result;

                if (stock == null || user == null) 
                    return Result<Portfolio>.Error(stock == null ? "Stock not Found" : "User not Found", 404);

                var portfolio = await _PortfolioRepo.GetPortfolio(user.Id, IdPortfolio);
                
                
                await UpdateHolding(user, stock, IdPortfolio);

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

            var user = usertask.Result;
            var stock = stocktask.Result;

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


        private async Task UpdateHolding(AppUser user, Stock Stock, int PortfolioID)
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
            }
        }

        private async Task<Stock?> GetStockAsync(String Symbol)
        {
            var Stock = await _StockRepo.GetbySymbolAsync(Symbol);

            if(Stock == null)
            {
                var SearchStock = await _FMPservice.FindBySymbolAsync(Symbol);

                if (SearchStock.Data == null) return null;

                EnqueuePublishStockFollowed(SearchStock.Data.Symbol);
                Stock = SearchStock.Data;
                await _StockRepo.Createasync(Stock);

                return Stock;
            }

            return Stock;
        }

        private async Task HandleStockUnfollowed (Stock stock)
        {
            bool IsFollowed = await _HoldingRepository.AnyUserHoldingStock(stock.Symbol);

            if (!IsFollowed)
            {
                await _StockRepo.Deleteasync(stock.ID);
                EnqueuePublishStockUnfollowed(stock.Symbol);
            }
        }

        private void EnqueuePublishStockFollowed(string symbol)
        {
            _TaskQueue.Enqueue(async token =>
            {
                try
                {
                    await _Publisher.PublishStockFollowedAsync(symbol);
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex, "Error occurred executing background task.");
                }
            });
        }

        private void EnqueuePublishStockUnfollowed(string symbol)
        {
            _TaskQueue.Enqueue(async token =>
            {   
                try
                {
                    await _Publisher.PublishStockUnfollowedAsync(symbol);
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex, "Error occurred executing background task.");
                }
            });
        }

        private bool ContainsStock(List<Stock> Portfolio, string symbol) =>
            Portfolio.Any(s => s.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));

       
    }
}