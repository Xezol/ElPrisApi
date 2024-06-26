using Azure.Messaging.ServiceBus;
using Common.Interfaces;
using Common.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackgroundWorker.Functions
{
    public class DailyPriceUpdateFunction
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusSender _serviceBusSender;
        private ITableBlobStorageFor<Prices> _prices { get; }

        public DailyPriceUpdateFunction(HttpClient httpClient,
            ILoggerFactory loggerFactory,
            ITableBlobStorageFor<Prices> prices,
            IConfiguration configuration,
            ServiceBusClient serviceBusClient)
        {
            _logger = loggerFactory.CreateLogger<DailyPriceUpdateFunction>();
            _configuration = configuration;
            _prices = prices;
            _httpClient = httpClient;
            _serviceBusClient = serviceBusClient;
            _serviceBusSender = _serviceBusClient.CreateSender(_configuration["%PriceUpdateQueueName%"]);
        }

        //Fetch todays prices
        [Function("DailyPriceUpdateFunction")]
        public async Task Run([TimerTrigger("%DailyPriceUpdateCronSchema%")] MyInfo myTimer,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation($"DailyPriceUpdateFunction executed at: {DateTime.Now}");

            var partitionKey = DateTime.Now.ToShortDateString();

            var todaysPrices = _prices.FetchAsync(partitionKey);

            if (todaysPrices == null)
            {
                var message = new ServiceBusMessage(partitionKey);
                await _serviceBusSender.SendMessageAsync(message);
            }
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        }
    }
}
