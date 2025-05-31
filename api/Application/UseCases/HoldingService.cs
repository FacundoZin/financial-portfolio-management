using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Common;
using api.Application.DTOs.Stock;
using api.Application.Interfaces.Infrastructure.Caching;
using api.Application.Interfaces.Infrastructure.FMP_Client;
using api.Application.Interfaces.Infrastructure.Messaging;
using api.Application.Interfaces.Infrastructure.Reposiories;
using api.Application.Interfaces.UseCases;
using api.Application.mappers;
using api.Domain.Entities;
using StackExchange.Redis;

namespace api.Application.UseCases
{
    public class HoldingService : IHoldingService
    {
        private readonly IaccountService _AccountService;
        private readonly IHoldingRepository _HoldingRepository;
        private readonly IStockRepository _StockRepo;
        private readonly IFMPService _FMPservice;
        private readonly IStockFollowPublisher _Publisher;
        private readonly ILogger _Logger;
        private readonly IRedisStockTracker _RedisStocksCaching;

        public HoldingService(IaccountService accountservice, IHoldingRepository holdingRepository,
        IStockRepository stockrepo, IFMPService fMPService, IStockFollowPublisher publisher,ILogger logger, IRedisStockTracker redisStockCaching)
        {
            _AccountService = accountservice;
            _HoldingRepository = holdingRepository;
            _StockRepo = stockrepo;
            _FMPservice = fMPService;
            _Publisher = publisher;
            _RedisStocksCaching = redisStockCaching;
            _Logger = logger;
        }

        public async Task<Result<AddedstockToHolding>> AddStock(string username, string symbol)
        {
            try
            {
                Stock stock = new Stock();
                var TaskappUser = _AccountService.FindByname(username);
                var TaskStockExists = _RedisStocksCaching.StockExist(symbol);

                await Task.WhenAll(TaskappUser, TaskStockExists);

                var appUser = TaskappUser.Result;
                bool StockExists = TaskStockExists.Result;
                
                if (appUser == null)
                    return Result<AddedstockToHolding>.Error("user not found", 404);

                var UserHolding = await _HoldingRepository.GetHoldingByUser(appUser);

                if (StockExists)
                {
                    if (UserHolding != null && ContainsStock(UserHolding, symbol))
                        return Result<AddedstockToHolding>.Error("stock already added to holding", 409);

                    var taskcaching = _RedisStocksCaching.IncrementStockFollowersAsync(symbol);
                    var taskAddStockToholding = _HoldingRepository.AddStockToHolding(appUser.Id, await _RedisStocksCaching.GetStockID(symbol));

                    await Task.WhenAll(taskcaching, taskAddStockToholding);

                    stock = new Stock { Symbol = symbol};
                }

                if (!StockExists)
                {
                    var search = await _FMPservice.FindBySymbolAsync(symbol);
                    if (search.Data == null) return Result<AddedstockToHolding>.Error("Stock not found", 404);

                    var CreatedStock = await _StockRepo.Createasync(search.Data);


                    var cachingstock = _RedisStocksCaching.trackNewStock(CreatedStock.ID, symbol, search.Data.Industry, search.Data.Companyname);
                    var addstock = _HoldingRepository.AddStockToHolding(appUser.Id, CreatedStock.ID);

                    await Task.WhenAll(cachingstock, addstock);
                    stock = new Stock { Symbol = symbol };
                }

                var stockadded = stock.ToAddedStockHoldingfromStock();
                return Result<AddedstockToHolding>.Exito(stockadded);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, $"Unexpected error in AddStock for user {username} and symbol {symbol}");
                return Result<AddedstockToHolding>.Error("sorry, something went wrong", 500);
            }
        }

        public async Task<Result<Stock?>> DeleteStock(string username, string symbol)
        {
            try
            {
                var TaskappUser = _AccountService.FindByname(username);
                var Taskstockexist = _RedisStocksCaching.StockExist(symbol);

                await Task.WhenAll(TaskappUser, Taskstockexist);

                var appuser = TaskappUser.Result;
                var stockexist = Taskstockexist.Result;

                if (appuser == null) return Result<Stock?>.Error("user not exit", 404);
                if (!stockexist) return Result<Stock?>.Error("stock not found", 404);

                var UserHolding = await _HoldingRepository.GetHoldingByUser(appuser);

                if (UserHolding == null || !ContainsStock(UserHolding, symbol))
                    return Result<Stock?>.Error(UserHolding == null ? "Nothing stock Added to Holding" : "The Stock Was not added", 404);

                var taskdecrementcount = _RedisStocksCaching.DecrementStockFollowersAsync(symbol);
                var taskdeleteholding = _HoldingRepository.DeleteHolding(appuser.Id, await _RedisStocksCaching.GetStockID(symbol));

                await Task.WhenAll(taskdecrementcount, taskdeleteholding);

                int stockfollowers = taskdecrementcount.Result;
                if (stockfollowers == 0) await _StockRepo.Deleteasync(await _RedisStocksCaching.GetStockID(symbol));
                return Result<Stock?>.Exito(null);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, $"Unexpected error in DeleteStock for user {username} and symbol {symbol}");
                return Result<Stock?>.Error("sorry, something went wrong", 500);
            }
        }

        public async Task<Result<List<StockDto>?>> GetHoldingUser(string username)
        {
            try
            {
                var appUser = await _AccountService.FindByname(username);

                if (appUser == null) return Result<List<StockDto>?>.Error("User not found", 404);

                var userstocks = await _HoldingRepository.GetHoldingByUser(appUser);

                if (userstocks == null) return Result<List<StockDto>?>.Exito(null);

                var Holding = userstocks.Select(s => s.toStockDto()).ToList();

                return Result<List<StockDto>?>.Exito(Holding);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, $"Unexpected error in GetAllStocks for user {username}");
                return Result<List<StockDto>?>.Error("we are sorry, something went wrong", 500);
            }
        }



        private bool ContainsStock(List<Stock> holdings, string symbol) =>
            holdings.Any(s => s.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));

    }
}