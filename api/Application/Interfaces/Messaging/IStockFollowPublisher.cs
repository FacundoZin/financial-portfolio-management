using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Application.Interfaces.Messaging
{
    public interface IStockFollowPublisher
    {
        Task PublishStockFollowedAsync(string symbol);
        Task PublishStockUnfollowedAsync(string symbol);
    }
}