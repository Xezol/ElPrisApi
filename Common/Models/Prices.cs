namespace Common.Models
{
    public class Prices
    {
        public List<PriceResults> PriceResults { get; set; } = [];
    }

    public class PriceResults
    {
        public Area Area { get; set; }
        public double SEK_per_kWh { get; set; }
        public double EUR_per_kWh { get; set; }
        public double EXR { get; set; }
        public DateTimeOffset TimeStart { get; set; }
        public DateTimeOffset TimeEnd { get; set; }
    }
}
