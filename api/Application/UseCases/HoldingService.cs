using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Common;
using api.Application.DTOs.Stock;
using api.Application.Interfaces.Infrastructure.BackgrounServices;
using api.Application.Interfaces.Infrastructure.FMP_Client;
using api.Application.Interfaces.Infrastructure.Messaging;
using api.Application.Interfaces.Infrastructure.Reposiories;
using api.Application.Interfaces.UseCases;
using api.Application.mappers;
using api.Domain.Entities;

namespace api.Application.UseCases
{
    public class HoldingService : IHoldingService
    {
        private readonly IaccountService _AccountService;
        private readonly IHoldingRepository _HoldingRepository;
        private readonly IStockRepository _StockRepo;
        private readonly IFMPService _FMPservice;
        private readonly IStockFollowPublisher _Publisher;
        private readonly IBackgroundTaskQueue _TaskQueue;
        private readonly ILogger _Logger;

        public HoldingService(IaccountService accountservice, IHoldingRepository holdingRepository,
        IStockRepository stockrepo, IFMPService fMPService, IStockFollowPublisher publisher, IBackgroundTaskQueue TaskQueue,
        ILogger logger)
        {
            _AccountService = accountservice;
            _HoldingRepository = holdingRepository;
            _StockRepo = stockrepo;
            _FMPservice = fMPService;
            _Publisher = publisher;
            _TaskQueue = TaskQueue;
            _Logger = logger;
        }

        public async Task<Result<AddedstockToHolding>> AddStock(string username, string symbol)
        {
            try
            {
                var TaskappUser = GetUser(username);
                var Taskstock = GetOrRetrieveStockAsync(symbol);

                await Task.WhenAll(TaskappUser, Taskstock);

                var appUser = TaskappUser.Result;
                var stock = Taskstock.Result;

                if (appUser == null || stock == null)
                    return Result<AddedstockToHolding>.Error(appUser == null ? "user not found" : "stock not found", 404);

                var UserHolding = await _HoldingRepository.GetHoldingByUser(appUser);

                if (UserHolding != null && ContainsStock(UserHolding, symbol))
                    return Result<AddedstockToHolding>.Error("stock already added to holding", 409);

                bool result = await _HoldingRepository.AddStockToHolding(appUser, stock);

                if (result == false)
                {
                    throw new Exception($"Unexpected error in AddStock for user {username} and symbol {symbol}");
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
                var TaskappUser = GetUser(username);
                var Taskstock = _StockRepo.GetbySymbolAsync(symbol);

                await Task.WhenAll(TaskappUser, Taskstock);

                var appuser = TaskappUser.Result;
                var stock = Taskstock.Result;

                if (appuser == null) return Result<Stock?>.Error("user not exit", 404);
                if (stock == null) return Result<Stock?>.Error("stock not found", 404);

                var UserHolding = await _HoldingRepository.GetHoldingByUser(appuser);

                if (UserHolding == null || !ContainsStock(UserHolding, symbol))
                    return Result<Stock?>.Error(UserHolding == null ? "Nothing stock Added to Holding" : "The Stock Was not added", 404);

                bool result = await _HoldingRepository.DeleteStock(appuser, stock);
                if (result == false) 
                {
                    throw new Exception($"Unexpected error in DeleteStock for user {username} and symbol {symbol}");
                }

                await HandleUnfollowedStocks(stock);

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
                var appUser = await GetUser(username);

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
                    _Logger.LogError(ex, "An error occurred while running the background task to publish the followed stock.");
                }
            });
        }

        private void EnqueuePublishStockUnfollwed (string symbol)
        {
            _TaskQueue.Enqueue(async token =>
            {
                try
                {
                    await _Publisher.PublishStockUnfollowedAsync(symbol);

                }catch (Exception ex)
                {
                    _Logger.LogError(ex, "An error occurred while running the background task to publish the unfollowed stock.");
                }
            });
        }

        private async Task<Stock?> GetOrRetrieveStockAsync (string symbol)
        {
            var stock = await _StockRepo.GetbySymbolAsync(symbol);

            if (stock == null)
            {
                var result = await _FMPservice.FindBySymbolAsync(symbol);
                if (result.Data == null) return null;

                EnqueuePublishStockFollowed (result.Data.Symbol);
                stock = result.Data;
                return stock;
            }

            return stock;
        }

        private async Task<AppUser?> GetUser(string Username)
        {
            var user = await _AccountService.FindByname(Username);

            if (user == null) return null;
            return user;
        }

        private bool ContainsStock(List<Stock> holdings, string symbol) =>
            holdings.Any(s => s.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));

        private async Task HandleUnfollowedStocks(Stock stock)
        {
            bool isFollowed = await _HoldingRepository.AnyUserHoldingStock(stock.Symbol);

            if (!isFollowed)
            {
                await _StockRepo.Deleteasync(stock.ID);
                EnqueuePublishStockUnfollwed(stock.Symbol);
            }

        }
    }
}