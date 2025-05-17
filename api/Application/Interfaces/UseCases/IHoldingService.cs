using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Common;
using api.Application.DTOs.Stock;
using api.Domain.Entities;

namespace api.Application.Interfaces.UseCases
{
    public interface IHoldingService
    {
        Task<Result<List<StockDto>?>> GetHoldingUser(string username);
        Task<Result<AddedstockToHolding>> AddStock(string username, string symbol);
        Task<Result<Stock?>> DeleteStock(string username, string symbol);

    }
}