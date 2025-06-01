using api.Application.Interfaces.Infrastructure.Caching;
using api.Domain.Entities;
using StackExchange.Redis;
using System.Collections.Generic;

namespace api.Infrastructure.Caching
{
    public class RedisStockTracker : IRedisStockTracker
    {
        private readonly IDatabase _db;
        private const string KeyPrefix = "stock:data:";

        public RedisStockTracker(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        private string GetStockHashKey(string symbol) => $"{KeyPrefix}{symbol}";

        public async Task<int> GetStockFollowersCountAsync(string symbol)
        {
            var hashkey = GetStockHashKey(symbol);
            var followersValue = await _db.HashGetAsync(hashkey, "followers");

            if (int.TryParse(followersValue.ToString(), out int followersCount))
            {
                return followersCount;
            }
            else
            {
                return 0;
            }
        }

        public async Task IncrementStockFollowersAsync(string symbol)
        {
            var hashKey = GetStockHashKey(symbol);

            var TaskIncrementfollowerscount = await _db.HashIncrementAsync(hashKey, "followers", 1);
        }

        public async Task<int> DecrementStockFollowersAsync(string symbol)
        {
            var HaskKey = GetStockHashKey(symbol);
            var count = await _db.HashDecrementAsync(HaskKey, "followers", 1);

            if ((int)count == 0)
            {
                await _db.KeyDeleteAsync(HaskKey);
            }

            return (int)count;
        }

        public async Task<bool> StockExist(string symbol)
        {
            return await _db.KeyExistsAsync(GetStockHashKey(symbol));
        }

        public async Task<int> GetStockID(string symbol)
        {
            var hashKey = GetStockHashKey(symbol);
            RedisValue result = await _db.HashGetAsync(hashKey, "id"); 

            if (result.HasValue && int.TryParse(result.ToString(), out int stockID))
            {
                return stockID;
            }
            else
            {
                throw new Exception("error getting stockid in redis");
            }
        }

        public async Task<Stock> GetStockData(string symbol)
        {
            var hashKey = GetStockHashKey(symbol);
            RedisValue id = await _db.HashGetAsync(hashKey, "id");
            RedisValue followers = await _db.HashGetAsync(hashKey, "followers");
            RedisValue industry = await _db.HashGetAsync(hashKey, "industy");
            RedisValue companyname = await _db.HashGetAsync(hashKey, "companyname");

            return new Stock
            {
                ID = (int)id,
                followers = (int)followers,
                Symbol = symbol,
                Industry = industry.ToString(),
                Companyname = companyname.ToString(),
            };
        }

        public async Task trackNewStock(int stockID, string symbol, string industry, string companyname)
        {
            var hashKey = GetStockHashKey(symbol);

            var Incrementfollowerscount = _db.HashIncrementAsync(hashKey, "followers", 1);
            var SetStockID = _db.HashSetAsync(hashKey, "id", stockID.ToString());
            var SetIndustry = _db.HashSetAsync(hashKey, "industry", symbol);
            var SetCompanyname = _db.HashSetAsync(hashKey, "companyname", companyname);

            await Task.WhenAll(Incrementfollowerscount, SetStockID, SetIndustry, SetCompanyname, SetIndustry);
        }
    }
}
