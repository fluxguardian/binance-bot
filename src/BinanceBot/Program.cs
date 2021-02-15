using Binance.Net;
using Binance.Net.Objects.Spot;
using CryptoExchange.Net.Authentication;
using System;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace BinanceBot
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<Program>();

            Configuration = builder.Build();

            var secretProvider = Configuration.Providers.First();
            if (!secretProvider.TryGet("ApiKey", out var apiKey)) return;
            if (!secretProvider.TryGet("SecretKey", out var secretKey)) return;

            var client = new BinanceClient(new BinanceClientOptions
            {
                ApiCredentials = new ApiCredentials(apiKey, secretKey)
            });

            var startUserStreamResult = client.Spot.UserStream.StartUserStream();

            if (!startUserStreamResult.Success)
                throw new Exception($"Failed to start user stream: {startUserStreamResult.Error}");

            var socketClient = new BinanceSocketClient();
            // subscribe to updates on the spot API
            socketClient.Spot.SubscribeToBookTickerUpdates("BTCEUR", data => {
                // Handle data
                //Console.WriteLine(data.BestBidPrice);
            });

            socketClient.Spot.SubscribeToUserDataUpdates(startUserStreamResult.Data,
                accountUpdate =>
                { // Handle account info update 
                },
                orderUpdate =>
                { // Handle order update
                },
                ocoUpdate =>
                { // Handle oco order update
                },
                positionUpdate =>
                { // Handle account position update
                    Console.WriteLine(positionUpdate.Balances.First().Total);
                },
                balanceUpdate =>
                { // Handle balance update
                    Console.WriteLine(balanceUpdate.BalanceDelta);
                });
            Console.ReadKey();
        }
    }
}
