using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Common;
using api.Domain.Entities;
using api.Application.DTOs.Stock;
using api.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using api.Application.Interfaces.Infrastructure.Reposiories;

namespace api.Infrastructure.Persistence.Repository
{
    public class HoldingRepository : IHoldingRepository
    {
        private ApplicationDBcontext _DBcontext;

        public HoldingRepository(ApplicationDBcontext dBcontext)
        {
            _DBcontext = dBcontext;
        }

        public async Task AddStockToHolding(string UserID, int stockID)
        {
            Holding added_item = new Holding
            {
                StockID = stockID,
                AppUserID = UserID
            };
            
            await _DBcontext.Holdings.AddAsync(added_item);
            int rowsaffected = await _DBcontext.SaveChangesAsync();

            if (rowsaffected == 0) throw new Exception("Error adding stock to holding");
        }

        public async Task<List<Stock>?> GetHoldingByUser(AppUser User)
        {
            var Holding = await _DBcontext.Holdings.Where(p => p.AppUserID == User!.Id)
            .Select(S => new Stock
            {
                ID = S.StockID,
                Symbol = S.Stock.Symbol,
                Companyname = S.Stock.Companyname,
                Purchase = S.Stock.Purchase,
                LastDiv = S.Stock.LastDiv,
                Industry = S.Stock.Industry,
                MarketCap = S.Stock.MarketCap
            }).ToListAsync();

            if (Holding == null) return null;

            return Holding!;
        }

        public async Task DeleteHolding(string UserID, int stockID)
        {
            Holding delete_item = new Holding
            {
                StockID = stockID,
                AppUserID = UserID,
            };

            _DBcontext.Holdings.Remove(delete_item);
            int rowsAffected = await _DBcontext.SaveChangesAsync();

            if (rowsAffected == 0) throw new Exception("Error deletting stock from holding");
        }

        public async Task<bool> addrelationship_withportfolio(Holding Updated_Holding)
        {
            _DBcontext.Holdings.Update(Updated_Holding);

            var affectedRows = await _DBcontext.SaveChangesAsync();

            if (affectedRows == 0) return false;

            return true;
        }

        public async Task<bool> AnyUserHoldingStock(string Symbol)
        {
            return await _DBcontext.Holdings.AnyAsync(H => H.Stock.Symbol == Symbol);
        }
    }
}