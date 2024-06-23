namespace ElPrisApi.Models
{

    /// <summary>
    /// Return class from elprisetjustnu api
    /// {
    ///     "SEK_per_kWh": 0.0472,
    ///     "EUR_per_kWh": 0.00413,
    ///     "EXR": 11.428426,
    ///     "time_start": "2024-06-02T00:00:00+02:00",
    ///     "time_end": "2024-06-02T01:00:00+02:00"
    ///   },
    /// </summary>
    public class Price
    {
        public double SEK_per_kWh { get; set; }
        public double EUR_per_kWh { get; set; }
        public double EXR { get; set; }
        public DateTimeOffset time_start { get; set; }
        public DateTimeOffset time_end { get; set; }

    }
}
