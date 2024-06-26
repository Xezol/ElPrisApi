using BackgroundWorker.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackgroundWorker.Functions
{
    public class CleanOutOldRecordsFunction
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public CleanOutOldRecordsFunction(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<CleanOutOldRecordsFunction>();
            _configuration = configuration;
        }

        [Function("CleanOutOldRecordsFunction")]
        public async Task Run([TimerTrigger("%TablesToCleanCronSchema%")] MyInfo myTimer)
        {
            _logger.LogInformation($"CleanOutOldRecordsFunction executed at: {DateTime.Now}");

            var tablesToClean = _configuration["TablesToClean"].Split(',');
            var daysToRetains = _configuration["DaysToRetain"].Split(',');


            for (int i = 0; i < tablesToClean.Length; i++)
            {
                int days = int.Parse(daysToRetains[i]);
                var table = tablesToClean[i];
                _logger.LogInformation($"Cleaning records older than {days} days from {table}.");
                var tableStorage = new AzureTableStorage(Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? "UseDevelopmentStorage=true", table);

                var date = DateTime.Today.AddDays(-days);
                for (int x = 0; x < 10; x++)
                {
                    await tableStorage.DeleteRowsByPartitionKeyAsync(date);
                    date.AddDays(-1);
                }
            }

            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        }
    }

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }
        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }
        public DateTime Next { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
