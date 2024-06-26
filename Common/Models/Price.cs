using System.Text.Json.Serialization;

namespace Common.Models
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
        [JsonPropertyName("SEK_per_kWh")]
        public double SEK_per_kWh { get; set; }

        [JsonPropertyName("EUR_per_kWh")]
        public double EUR_per_kWh { get; set; }

        [JsonPropertyName("EXR")]
        public double EXR { get; set; }

        [JsonPropertyName("time_start")]
        public DateTimeOffset time_start { get; set; }

        [JsonPropertyName("time_end")]
        public DateTimeOffset time_end { get; set; }
    }
}
