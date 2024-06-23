using ElPrisApi;
using ElPrisApi.Constants;
using ElPrisApi.Interfaces;
using ElPrisApi.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

public class PriceService : IPriceService
{
    private readonly HttpClient _httpClient;
    private readonly ApiSettings _apiSettings;
    private readonly ITableStorageService _tableStorageService;

    public PriceService(HttpClient httpClient, IOptions<ApiSettings> apiSettings, ITableStorageService tableStorageService)
    {
        _httpClient = httpClient;
        _apiSettings = apiSettings.Value;
        _tableStorageService = tableStorageService;
    }

    public async Task<List<Price>> GetPricesForTodayAsync(string priceClass)
    {
        var today = DateTime.Now;
        string year = today.Year.ToString();
        string month = today.Month.ToString("D2");
        string day = today.Day.ToString("D2");
        string partitionKey = $"{priceClass}-{year}-{month}-{day}";

        var storedEntities = await _tableStorageService.GetPriceEntitiesAsync(partitionKey);

        if (storedEntities.Any())
        {
            return storedEntities.Select(entity => new Price
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
            string url = $"{_apiSettings.BaseUrl}/{year}/{month}-{day}_{priceClass}.json";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var prices = JsonSerializer.Deserialize<List<Price>>(jsonResponse, JsonConstants.DefaultJsonSerializerOptions);

            if (prices != null && prices.Count > 0)
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

                await _tableStorageService.UpsertPriceEntitiesAsync(entities);
            }

            return prices ?? [];
        }
    }
}