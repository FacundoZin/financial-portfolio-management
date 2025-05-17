using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Common;
using api.Domain.Entities;

namespace api.Application.Interfaces.Infrastructure.FMP_Client
{
    public interface IFMPService
    {
        Task<Result<Stock>> FindBySymbolAsync(string symbol);
    }
}