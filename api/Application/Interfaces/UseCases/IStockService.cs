using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Common;
using api.Application.DTOs.Stock;
using api.Application.Helpers;
using api.Domain.Entities;

namespace api.Application.Interfaces.UseCases
{
    public interface IStockService
    {
        Task<List<StockDto?>> GetAllStocksAsync(StockQueryObject queryObject);
        Task<Result<StockDto?>> GetStockByIdAsync(int id);
        Task<Result<Stock>> CreateStockAsync(CreateStockDto createStock);
        Task<Result<Stock?>> DeleteStockAsync(int id);
        Task<Result<StockDto?>> UpdateStockAsync(int id, UpdateStockDto updateStockDto);
    }
}