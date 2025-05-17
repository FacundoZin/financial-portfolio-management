using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using api.Application.Common;
using api.Application.DTOs.Stock;
using api.Application.Interfaces.Infrastructure.FMP_Client;
using api.Application.mappers;
using api.Domain.Entities;
using Newtonsoft.Json;

namespace api.Infrastructure.Services
{
    public class FMP_Client : IFMPService
    {
        private HttpClient _HttpClient;
        private IConfiguration _Config;

        public FMP_Client(HttpClient httpClient, IConfiguration config)
        {
            _HttpClient = httpClient;
            _Config = config;
        }


        public async Task<Result<Stock>> FindBySymbolAsync(string symbol)
        {
            try
            {
                var result = await _HttpClient.GetAsync($"https://financialmodelingprep.com/api/v3/profile/{symbol}?apikey={_Config["FMPkey"]}");

                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    var tasks = JsonConvert.DeserializeObject<FMPstockDto[]>(content);
                    var Stock = tasks[0];

                    if (Stock != null)
                    {
                        return Result<Stock>.Exito(Stock.TOstockFromFMP());
                    }

                    return Result<Stock>.Error("stock not found", (int)result.StatusCode);
                }
                return Result<Stock>.Error("something went wrong", (int)result.StatusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Result<Stock>.Error("something went wrong", 500);
            }
        }
    }
}