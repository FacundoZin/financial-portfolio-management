using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using api.Application.Interfaces.Infrastructure.Messaging;
using api.Application.UseCases;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace api.Infrastructure.Messaging
{
    public class StockPublisher : IStockFollowPublisher
    {
        private readonly IConnection _Connection;

        public async Task PublishStockFollowedAsync(string symbol)
        {
            string exchange = "Stocks_Followed";
            string routingKey = $"Stock.Follow.{symbol}";

            var chanel = await initChanel();

            await Publish(chanel, symbol, routingKey, exchange);
        }

        public async Task PublishStockUnfollowedAsync(string symbol)
        {
            string exchange = "Stocks_Followed";
            string routingKey = $"Stock.Unfollowed.{symbol}";

            var chanel = await initChanel();

            await Publish(chanel, symbol, routingKey, exchange);
        }

        private async Task Publish(IChannel chanel, string symbol, string routingKey, string exchange)
        {
            await chanel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true);

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(symbol));

            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            await chanel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: props,
                body: body
                );
        }

        private async Task<IChannel> initChanel()
        {
            var channel = await _Connection.CreateChannelAsync();
            return channel;
        }

    }
}