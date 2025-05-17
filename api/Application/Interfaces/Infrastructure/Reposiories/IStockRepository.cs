using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.DTOs.Stock;
using api.Domain.Entities;
using api.Application.Helpers;

namespace api.Application.Interfaces.Infrastructure.Reposiories
{

    public interface IStockRepository
    {
        IQueryable<Stock> GetAllStocks();
        Task<Stock?> Getbyidasync(int id);
        Task<Stock?> GetbySymbolAsync(string symbol);
        Task<Stock?> Createasync(Stock stock);
        Task<Stock?> Updateasync(int id, UpdateStockDto updatedstock);
        Task<Stock?> Deleteasync(int id);
        Task<bool> StockExist(int id);
    }
}