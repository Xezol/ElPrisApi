using Azure.Messaging.ServiceBus;
using Common.Interfaces;
using Common.Models;

namespace ElPrisApi.Services
{
    public class PriceService : IPriceService
    {
        private readonly IPriceApiFetcher _priceApiFetcher;
        private readonly ITableBlobStorageFor<Prices> _priceStorage;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _baseUrl;
        private readonly string _queueName;

        public PriceService(IPriceApiFetcher priceApiFetcher,
            ITableBlobStorageFor<Prices> priceStorage,
            ServiceBusClient serviceBusClient,
            string baseUrl,
            string queueName)
        {
            _priceApiFetcher = priceApiFetcher;
            _priceStorage = priceStorage;
            _serviceBusClient = serviceBusClient;
            _baseUrl = baseUrl;
            _queueName = queueName;
        }

        public async Task<PriceSummary> GetPricesForTodayAsync(Area area)
        {
            var partitionKey = DateTime.Now.ToShortDateString();
            var storedEntities = await _priceStorage.FetchAsync(partitionKey);

            List<Price> prices;

            if (storedEntities != null && storedEntities.PriceResults.Any())
            {
                prices = storedEntities.PriceResults
                    .Where(p => p.Area == area)
                    .Select(entity => new Price
                    {
                        SEK_per_kWh = entity.SEK_per_kWh,
                        EUR_per_kWh = entity.EUR_per_kWh,
                        EXR = entity.EXR,
                        time_start = entity.TimeStart,
                        time_end = entity.TimeEnd
                    }).ToList();
            }
            else
            {
                await QueueServiceBusMessageAsync(area);
                prices = await _priceApiFetcher.GetPricesForTodayAndArea(area, _baseUrl);
            }

            var averagePrice = prices.Average(p => p.SEK_per_kWh);
            var highestPrice = prices.Max(p => p.SEK_per_kWh);
            var lowestPrice = prices.Min(p => p.SEK_per_kWh);

            return new PriceSummary
            {
                Area = area.ToString(),
                Prices = prices,
                AveragePrice = averagePrice,
                HighestPrice = highestPrice,
                LowestPrice = lowestPrice
            };
        }


        private async Task QueueServiceBusMessageAsync(Area area)
        {
            var messageBody = new
            {
                Area = area.ToString(),
            };

            var message = new ServiceBusMessage(System.Text.Json.JsonSerializer.Serialize(messageBody));

            var sender = _serviceBusClient.CreateSender(_queueName);
            await sender.SendMessageAsync(message);
        }
    }

}