using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Interfaces.Reposiories;
using api.Application.Common;
using api.Domain.Entities;
using api.Application.DTOs.Stock;
using api.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Infrastructure.Persistence.Repository
{
    public class HoldingRepository : IHoldingRepository
    {
        private ApplicationDBcontext _DBcontext;

        public HoldingRepository(ApplicationDBcontext dBcontext)
        {
            _DBcontext = dBcontext;
        }

        public async Task<bool> AddStockToHolding(AppUser User, Stock stock)
        {
            try
            {
                Holding added_item = new Holding
                {
                    StockID = stock.ID,
                    AppUserID = User.Id
                };

                await _DBcontext.Holdings.AddAsync(added_item);
                await _DBcontext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            ;
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

        public async Task<bool> DeleteStock(AppUser user, Stock stock)
        {
            Holding delete_item = new Holding
            {
                StockID = stock.ID,
                AppUserID = user.Id
            };

            _DBcontext.Holdings.Remove(delete_item);
            int rowsAffected = await _DBcontext.SaveChangesAsync();

            if (rowsAffected == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}