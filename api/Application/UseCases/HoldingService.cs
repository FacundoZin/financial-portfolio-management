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
                var TaskappUser = _AccountService.FindByname(username);
                var Taskstock = _StockRepo.GetbySymbolAsync(symbol);

                await Task.WhenAll(TaskappUser, Taskstock);

                var appUser = await TaskappUser;
                var stock = await Taskstock;

                if (appUser == null) return Result<AddedstockToHolding>.Error("user not found", 404);

                if (stock == null)
                {
                    var search = await _FMPservice.FindBySymbolAsync(symbol);

                    if (search.Data == null) return Result<AddedstockToHolding>.Error("this stock not exist", 404);

                    stock = search.Data;

                    EnqueuePublishStockFollowed(search.Data.Symbol);
                }

                bool result = await _HoldingRepository.AddStockToHolding(appUser, stock);

                if (result == false) return Result<AddedstockToHolding>.Error("sorry, something went wrong", 500);

                var stockadded = stock.ToAddedStockHoldingfromStock();

                return Result<AddedstockToHolding>.Exito(stockadded);
            }
            catch
            {
                return Result<AddedstockToHolding>.Error("sorry, something went wrong", 500);
            }
        }

        public async Task<Result<Stock?>> DeleteStock(string username, string symbol)
        {
            try
            {
                var TaskappUser = _AccountService.FindByname(username);
                var Taskstock = _StockRepo.GetbySymbolAsync(symbol);

                await Task.WhenAll(TaskappUser, Taskstock);

                var appuser = await TaskappUser;
                var stock = await Taskstock;

                if (appuser == null) return Result<Stock?>.Error("user not exit", 404);
                if (stock == null) return Result<Stock?>.Error("stock not exit", 404);

                var holding = await _HoldingRepository.GetHoldingByUser(appuser);
                if (holding.Count(s => s.Symbol.ToLower() == symbol.ToLower()) > 0) return Result<Stock?>.Error("the stock was not added to the portfolio", 400);

                bool result = await _HoldingRepository.DeleteStock(appuser, stock);
                if (result == false) return Result<Stock?>.Error("sorry, something went wrong", 500);


                return Result<Stock?>.Exito(null);

            }
            catch (Exception ex)
            {
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
                    _Logger.LogError(ex, "Error occurred executing background task.");
                }
            });
        }
    }
}