using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.DTOs.Stock;
using api.Domain.Entities;
using api.Application.Helpers;
using api.Infrastructure.Persistence.Data;
using api.Application.mappers;
using Microsoft.EntityFrameworkCore;
using api.Application.Interfaces.Infrastructure.Reposiories;

namespace api.Infrastructure.Persistence.Repository
{
    public class StockRepository : IStockRepository
    {
        private readonly ApplicationDBcontext _context;
        public StockRepository(ApplicationDBcontext context)
        {
            _context = context;
        }

        public async Task<Stock?> Createasync(Stock stock)
        {
            await _context.Stocks.AddAsync(stock);
            await _context.SaveChangesAsync();

            if (stock.ID == 0)
            {
                return null;

            }
            return stock;
        }

        public async Task<Stock?> Deleteasync(int id)
        {
            var stock = await _context.Stocks.FindAsync(id);

            if (stock == null)
            {
                return null;
            }

            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();

            return stock;
        }

        public IQueryable<Stock> GetAllStocks()
        {
            var consult = _context.Stocks.Include(c => c.Comments).ThenInclude(a => a.AppUser).AsQueryable();
            return consult;
        }

        public async Task<Stock?> Getbyidasync(int id)
        {
            var stock = await _context.Stocks.Include(c => c.Comments).FirstOrDefaultAsync(i => i.ID == id);

            if (stock == null)
            {
                return null;
            }

            return stock;
        }

        public async Task<Stock?> GetbySymbolAsync(string symbol)
        {
            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);

            if (stock == null)
            {
                return null;
            }

            return stock;
        }

        public async Task<bool> StockExist(int id)
        {
            return await _context.Stocks.AnyAsync(s => s.ID == id);
        }

        public async Task<Stock?> Updateasync(int id, UpdateStockDto updatedStock)
        {
            var stock = await _context.Stocks.FindAsync(id);

            if (stock == null)
            {
                return null;
            }

            stock.Symbol = updatedStock.Symbol;
            stock.Companyname = updatedStock.Companyname;
            stock.Purchase = updatedStock.Purchase;
            stock.LastDiv = updatedStock.LastDiv;
            stock.Industry = updatedStock.Industry;
            stock.MarketCap = updatedStock.MarketCap;

            await _context.SaveChangesAsync();

            return stock;
        }
    }
}