namespace ElPrisApi.Models
{
    public class PriceSummary
    {
        public string Area { get; set; } = null!;
        public double AveragePrice { get; set; }
        public double HighestPrice { get; set; }
        public double LowestPrice { get; set; }
        public List<Price> Prices { get; set; } = [];
    }
}
