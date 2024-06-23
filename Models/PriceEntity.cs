using Azure;
using Azure.Data.Tables;

namespace ElPrisApi.Models
{
    public class PriceEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = null!;
        public double SEK_per_kWh { get; set; }
        public double EUR_per_kWh { get; set; }
        public double EXR { get; set; }
        public DateTimeOffset TimeStart { get; set; }
        public DateTimeOffset TimeEnd { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
