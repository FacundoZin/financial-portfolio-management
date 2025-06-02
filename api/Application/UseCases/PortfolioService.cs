using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Common;
using api.Application.DTOs.Portfolio;
using api.Application.Interfaces.Infrastructure.Caching;
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
        private readonly IStockFollowPublisher _Publisher;
        private readonly ILogger _Logger;
        private readonly IRedisStockTracker _StockCaching;

        public PortfolioService(IPortfolioRepository portfolioRepository, IaccountService accountservice,
            IStockRepository stockRepository, IFMPService fMPService, IHoldingRepository holdingRepository, 
            IStockFollowPublisher publisher, ILogger logger, IRedisStockTracker stockcaching)
        {
            _PortfolioRepo = portfolioRepository;
            _AccountService = accountservice;
            _StockRepo = stockRepository;
            _FMPservice = fMPService;
            _HoldingRepository = holdingRepository;
            _Publisher = publisher;
            _Logger = logger;
            _StockCaching = stockcaching;
        }

        public async Task<Result<Portfolio>> AddStock(string username, string symbol, int IdPortfolio)
        {
            try
            {
                var stockexist = _StockCaching.StockExist(symbol);
                var usertask = _AccountService.FindByname(username);

                await Task.WhenAll(stockexist, usertask);

                bool StockExistResult = stockexist.Result;
                var appuser = usertask.Result;

                if (appuser == null) return Result<Portfolio>.Error("user not found", 404);

                if (!StockExistResult)
                {
                    bool Result = await HandleAddingStockUnfollowed(appuser, symbol, IdPortfolio);
                    if(!Result) return Result<Portfolio>.Error("stock no exist", 404);
                }

                var stock = await _StockCaching.GetStockData(symbol);

                var TaskGetportfolio = _PortfolioRepo.GetPortfolio(appuser.Id, IdPortfolio);
                var TaskGetholding = _HoldingRepository.GetHoldingByUser(appuser);

                await Task.WhenAll(TaskGetholding, TaskGetportfolio);

                var portfolio = TaskGetportfolio.Result;
                var holding = TaskGetholding.Result;

                if (holding.Any(s => s.ID == stock.ID))
                {
                    if (portfolio.Holdings.Any(s => s.StockID == stock.ID)) return Result<Portfolio>.Error("the stock has already been added", 409);
                    portfolio.Holdings.Add(new Holding { PortfolioID = IdPortfolio, AppUserID = appuser.Id, StockID = stock.ID });
                    await _PortfolioRepo.AddStockToPortfolio(portfolio);
                }

                portfolio.Holdings.Add(new Holding { PortfolioID = IdPortfolio, AppUserID = appuser.Id, StockID = stock.ID });

                var TaskAddStocktoPortfolio = _PortfolioRepo.AddStockToPortfolio(portfolio);
                var TaskAddStocktoHolding = _HoldingRepository.AddStockToHolding(appuser.Id, stock.ID, IdPortfolio);
                await Task.WhenAll(TaskAddStocktoPortfolio, TaskAddStocktoHolding);

                return Result<Portfolio>.Exito(null);

            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "error ocurred while added stock to portfolio");
                return Result<Portfolio>.Error("inteernal server error", 500);
            }
        }

        public async Task<Result<PortfolioAddedToUser>> CreatePortfolio(string username, string namePortfolio)
        {
            if (string.IsNullOrEmpty(namePortfolio)) return Result<PortfolioAddedToUser>.Error("please enter the name portfolio", 400);

            var user = await _AccountService.FindByname(username);

            if (user == null) return Result<PortfolioAddedToUser>.Error("user not found", 404);

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
            try
            {
                var usertask = _AccountService.FindByname(username);
                var stocktask = _StockCaching.StockExist(symbol);

                await Task.WhenAll(usertask, stocktask);

                var user = usertask.Result;
                var stockexist = stocktask.Result;

                if (user == null || stockexist == false)
                    return Result<Portfolio>.Error(user == null ? "user not found" : "stock not found", 404);


                var Taskportfolio = _PortfolioRepo.GetPortfolio(user.Id, IdPortfolio);
                var Taskholding = _HoldingRepository.GetHoldingByUser(user);
                var Taskstockdata = _StockCaching.GetStockData(symbol);


                await Task.WhenAll(Taskportfolio, Taskholding, Taskstockdata);

                var holding = Taskholding.Result;
                var portfolio = Taskportfolio.Result;
                var StockData = Taskstockdata.Result;

                
                if (!portfolio.Holdings.Any(s => s.StockID == StockData.ID))
                {
                    return Result<Portfolio>.Error("the stock has not been added to portfolio", 409);
                }

                portfolio.Holdings.Remove(new Holding { PortfolioID = IdPortfolio, AppUserID = user.Id, StockID = StockData.ID });

                var TaskDeleteStocktoHolding= _HoldingRepository.DeleteHolding(user.Id, StockData.ID);
                var TaskDeleteStocktoPortfolio = _PortfolioRepo.DeleteStock(new Holding { PortfolioID = IdPortfolio, AppUserID = user.Id, StockID = StockData.ID });

                await Task.WhenAll(TaskDeleteStocktoHolding,TaskDeleteStocktoPortfolio);

                if (StockData.followers == 1)
                {
                    var TaskDeleteStockCache = _StockCaching.DecrementStockFollowersAsync(symbol);
                    var TaskDeleteStock = _StockRepo.Deleteasync(StockData.ID);

                    await Task.WhenAll (TaskDeleteStock, TaskDeleteStockCache);
                }

                var DecrementStockCache = _StockCaching.DecrementStockFollowersAsync(symbol);


                return Result<Portfolio>.Exito(null);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error when deleted Stock from portfolio");
                return Result<Portfolio>.Error("Sorry Something went wrognt", 500);
            }
            
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


        private bool ContainsStock(List<Stock> Portfolio, string symbol) =>
            Portfolio.Any(s => s.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));  
        
        private async Task<bool> HandleAddingStockUnfollowed(AppUser user, string symbol, int idPortfolio)
        {
            try
            {
                var StockSearch = await _FMPservice.FindBySymbolAsync(symbol);

                if (StockSearch == null) return false;

                var stock = await _StockRepo.Createasync(StockSearch);

                var _portfolio = await _PortfolioRepo.GetPortfolio(user.Id, idPortfolio);
                _portfolio.Holdings.Add(new Holding { AppUserID = user.Id, StockID = stock.ID, PortfolioID = idPortfolio });

                var TaskAddStockToPortfolio = _PortfolioRepo.AddStockToPortfolio(_portfolio);
                var TaskAddStockToHolding = _HoldingRepository.AddStockToHolding(user.Id, stock.ID, idPortfolio);
                var TaskCachingStock = _StockCaching.trackNewStock(stock.ID, symbol, stock.Industry, stock.Companyname);

                await Task.WhenAll(TaskAddStockToPortfolio, TaskAddStockToHolding, TaskCachingStock);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }     
        }
    }
}