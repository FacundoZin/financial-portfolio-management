using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Common;
using api.Domain.Entities;

namespace api.Application.Interfaces.Infrastructure.Reposiories
{
    public interface IHoldingRepository
    {
        Task<List<Stock>?> GetHoldingByUser(AppUser User);
        Task AddStockToHolding(string UserID, int stockID);
        Task DeleteHolding(string UserID, int stockID);
        Task<bool> addrelationship_withportfolio(Holding Updated_Holding);
        Task<bool> AnyUserHoldingStock(string Symbol);
    }
}