using Common.Constants;
using Common.Interfaces;
using Common.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BackgroundWorker.Functions
{
    public class UpdatePriceQueueListenerFunction
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private ITableBlobStorageFor<Prices> _prices { get; }

        public UpdatePriceQueueListenerFunction(HttpClient httpClient,
            ILoggerFactory loggerFactory,
            ITableBlobStorageFor<Prices> prices,
            IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<UpdatePriceQueueListenerFunction>();
            _configuration = configuration;
            _prices = prices;
            _httpClient = httpClient;
        }

        [Function("UpdatePriceQueueListenerFunction")]
        public async Task Run([ServiceBusTrigger("%PriceUpdateQueueName%", Connection = "ServiceBusConnectionString")] string message,
            FunctionContext context)
        {
            var log = context.GetLogger("UpdatePriceQueueListenerFunction");
            log.LogInformation($"Received message: {message}");

            var partitionKey = DateTime.Now.ToShortDateString();

            var todaysPrices = await _prices.FetchAsync(partitionKey);

            if (todaysPrices == null)
            {
                List<PriceResults> priceResults = new List<PriceResults>();

                var areas = Enum.GetValues(typeof(Area)).Cast<Area>().ToList();

                foreach (var area in areas)
                {
                    var today = DateTime.Now;
                    string year = today.Year.ToString();
                    string month = today.Month.ToString("D2");
                    string day = today.Day.ToString("D2");

                    string url = $"{_configuration["BaseUrl"]}api/v1/prices/{year}/{month}-{day}_{area}.json";
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var prices = JsonSerializer.Deserialize<List<Price>>(jsonResponse, JsonConstants.DefaultJsonSerializerOptions) ?? new List<Price>();

                    if (prices.Count > 0)
                    {
                        priceResults.AddRange(prices.Select(price => new PriceResults
                        {
                            Area = area,
                            SEK_per_kWh = price.SEK_per_kWh,
                            EUR_per_kWh = price.EUR_per_kWh,
                            EXR = price.EXR,
                            TimeStart = price.time_start,
                            TimeEnd = price.time_end
                        }).ToList());
                    }
                }

                await _prices.SaveAsync(new Prices
                {
                    PriceResults = priceResults
                }, partitionKey);
            }
        }
    }
}
