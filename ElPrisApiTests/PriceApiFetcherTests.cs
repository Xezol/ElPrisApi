using Common.Models;
using Common.Services;
using RichardSzalay.MockHttp;

namespace ElPrisApiTests
{
    [TestClass]
    public class PriceApiFetcherTests
    {
        private MockHttpMessageHandler _mockHttp;
        private HttpClient _httpClient;
        private PriceApiFetcher _priceApiFetcher;
        private const string BaseUrl = "http://example.com/";

        [TestInitialize]
        public void Setup()
        {
            _mockHttp = new MockHttpMessageHandler();
            _httpClient = _mockHttp.ToHttpClient();
            _priceApiFetcher = new PriceApiFetcher(_httpClient);
        }

        [TestMethod]
        public async Task GetPricesForTodayAndArea_ReturnsPrices_OnSuccess()
        {
            // Arrange
            var area = Area.SE1;
            var responseContent = "[{\"SEK_per_kWh\": 1.0}]";
            var today = DateTime.Now;
            _mockHttp.When($"{BaseUrl}api/v1/prices/{today:yyyy}/{today:MM-dd}_{area}.json")
                .Respond("application/json", responseContent);

            // Act
            var result = await _priceApiFetcher.GetPricesForTodayAndArea(area, BaseUrl);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1.0, result.First().SEK_per_kWh);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task GetPricesForTodayAndArea_ThrowsException_OnHttpError()
        {
            // Arrange
            var area = Area.SE1;
            var today = DateTime.Now;
            _mockHttp.When($"{BaseUrl}api/v1/prices/{today:yyyy}/{today:MM-dd}_{area}.json")
                .Throw(new HttpRequestException());

            // Act
            await _priceApiFetcher.GetPricesForTodayAndArea(area, BaseUrl);
        }
    }
}
