using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Common;
using api.Application.DTOs.Stock;
using api.Application.Helpers;
using api.Application.Interfaces.Infrastructure.Reposiories;
using api.Application.Interfaces.UseCases;
using api.Application.mappers;
using api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.Application.UseCases
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _StockRepo;

        public StockService(IStockRepository stockRepository)
        {
            _StockRepo = stockRepository;
        }

        public async Task<Result<Stock>> CreateStockAsync(CreateStockDto createStock)
        {
            var stock = await _StockRepo.Createasync(createStock.ToStockfromCreateDto());

            if (stock == null)
            {
                return Result<Stock>.Error("something went wrong", 500);
            }
            else return Result<Stock>.Exito(stock);
        }

        public async Task<Result<Stock?>> DeleteStockAsync(int id)
        {
            var stock = await _StockRepo.Deleteasync(id);

            if (stock == null)
            {
                return Result<Stock?>.Error("stock no found", 404);
            }

            return Result<Stock?>.Exito(null);
        }

        public async Task<List<StockDto?>> GetAllStocksAsync(StockQueryObject queryObject)
        {
            var Stocks = _StockRepo.GetAllStocks();

            if (!string.IsNullOrWhiteSpace(queryObject.CompanyName))
                Stocks = Stocks.Where(s => s.Companyname.Contains(queryObject.CompanyName));

            if (!string.IsNullOrWhiteSpace(queryObject.Symbol))
                Stocks = Stocks.Where(s => s.Symbol.Contains(queryObject.Symbol));

            if (!string.IsNullOrWhiteSpace(queryObject.SortBy))
            {
                if (queryObject.SortBy.Equals("symbol", StringComparison.OrdinalIgnoreCase))
                {
                    if (queryObject.IsDecsending == true)
                    {
                        Stocks = Stocks.OrderByDescending(s => s.Symbol);
                    }
                    else
                    {
                        Stocks = Stocks.OrderBy(s => s.Symbol);
                    }
                }
            }

            var skipNumber = (queryObject.PageNumber - 1) * queryObject.PageSize;

            List<Stock> stock_list = await Stocks.Skip(skipNumber).Take(queryObject.PageSize).ToListAsync();

            return stock_list.Select(s => s.toStockDto()).ToList();
        }

        public async Task<Result<StockDto?>> GetStockByIdAsync(int id)
        {
            var stock = await _StockRepo.Getbyidasync(id);

            if (stock == null)
            {
                return Result<StockDto?>.Error("stock not found", 404);
            }
            else
            {
                return Result<StockDto?>.Exito(stock.toStockDto());
            }
        }

        public async Task<Result<StockDto?>> UpdateStockAsync(int id, UpdateStockDto updateStockDto)
        {
            var stock = await _StockRepo.Updateasync(id, updateStockDto);

            if (stock == null) return Result<StockDto?>.Error("stock not found", 404);

            return Result<StockDto?>.Exito(stock.toStockDto());
        }
    }
}