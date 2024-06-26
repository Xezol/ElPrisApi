using Common.Constants;
using Common.Interfaces;
using Common.Models;
using System.Text.Json;

namespace Common.Services
{
    public class PriceApiFetcher : IPriceApiFetcher
    {
        private readonly HttpClient _httpClient;

        public PriceApiFetcher(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<List<Price>> GetPricesForTodayAndArea(Area area, string baseUrl)
        {
            var today = DateTime.Now;
            string year = today.Year.ToString();
            string month = today.Month.ToString("D2");
            string day = today.Day.ToString("D2");

            string url = $"{baseUrl}api/v1/prices/{year}/{month}-{day}_{area}.json";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Price>>(jsonResponse, JsonConstants.DefaultJsonSerializerOptions) ?? new List<Price>();

        }
    }
}
