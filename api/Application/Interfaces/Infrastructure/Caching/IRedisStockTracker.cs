using api.Domain.Entities;

namespace api.Application.Interfaces.Infrastructure.Caching
{
    public interface IRedisStockTracker
    {
        Task trackNewStock(int stockID, string symbol, string industry, string companyname);
        Task IncrementStockFollowersAsync(string symbol);
        Task<int> DecrementStockFollowersAsync(string symbol);
        Task<int> GetStockFollowersCountAsync(string symbol);
        Task<bool> StockExist(string symbol);
        Task<int> GetStockID (string symbol);
        Task<Stock> GetStockData(string symbol);
    }
}
