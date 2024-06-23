using ElPrisApi.Constants;
using ElPrisApi.Interfaces;
using ElPrisApi.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ElPrisApi.Services
{
    public class PriceService : IPriceService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;
        private readonly ITableStorageService<PriceEntity> _tableStorageService;

        public PriceService(HttpClient httpClient, IOptions<ApiSettings> apiSettings, ITableStorageService<PriceEntity> tableStorageService)
        {
            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;
            _tableStorageService = tableStorageService;
        }

        public async Task<PriceSummary> GetPricesForTodayAsync(Area area)
        {
            var today = DateTime.Now;
            string year = today.Year.ToString();
            string month = today.Month.ToString("D2");
            string day = today.Day.ToString("D2");
            string partitionKey = $"{area}-{year}-{month}-{day}";

            var storedEntities = await _tableStorageService.GetEntitiesAsync(partitionKey);

            List<Price> prices;

            if (storedEntities.Any())
            {
                prices = storedEntities.Select(entity => new Price
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
                string url = $"{_apiSettings.BaseUrl}api/v1/prices/{year}/{month}-{day}_{area}.json";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                prices = JsonSerializer.Deserialize<List<Price>>(jsonResponse, JsonConstants.DefaultJsonSerializerOptions) ?? new List<Price>();

                if (prices.Count > 0)
                {
                    var entities = prices.Select(price => new PriceEntity
                    {
                        PartitionKey = partitionKey,
                        RowKey = price.time_start.ToString("HHmm"),
                        SEK_per_kWh = price.SEK_per_kWh,
                        EUR_per_kWh = price.EUR_per_kWh,
                        EXR = price.EXR,
                        TimeStart = price.time_start,
                        TimeEnd = price.time_end
                    }).ToList();

                    await _tableStorageService.UpsertEntitiesAsync(entities);
                }
            }

            var averagePrice = prices.Average(p => p.SEK_per_kWh);
            var highestPrice = prices.Max(p => p.SEK_per_kWh);
            var lowestPrice = prices.Min(p => p.SEK_per_kWh);

            return new PriceSummary
            {
                Prices = prices,
                AveragePrice = averagePrice,
                HighestPrice = highestPrice,
                LowestPrice = lowestPrice
            };
        }
    }

}