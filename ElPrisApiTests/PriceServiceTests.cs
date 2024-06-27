using Azure.Messaging.ServiceBus;
using Common.Interfaces;
using Common.Models;
using ElPrisApi.Services;
using Moq;

namespace ElPrisApiTests
{
    [TestClass]
    public class PriceServiceTests
    {
        private Mock<IPriceApiFetcher> _priceApiFetcherMock;
        private Mock<ITableBlobStorageFor<Prices>> _priceStorageMock;
        private Mock<ServiceBusClient> _serviceBusClientMock;
        private Mock<ServiceBusSender> _serviceBusSenderMock;
        private PriceService _priceService;
        private const string BaseUrl = "http://example.com/";
        private const string QueueName = "test-queue";

        [TestInitialize]
        public void Setup()
        {
            _priceApiFetcherMock = new Mock<IPriceApiFetcher>();
            _priceStorageMock = new Mock<ITableBlobStorageFor<Prices>>();
            _serviceBusClientMock = new Mock<ServiceBusClient>();
            _serviceBusSenderMock = new Mock<ServiceBusSender>();

            _serviceBusClientMock.Setup(client => client.CreateSender(It.IsAny<string>()))
                .Returns(_serviceBusSenderMock.Object);

            _priceService = new PriceService(
                _priceApiFetcherMock.Object,
                _priceStorageMock.Object,
                _serviceBusClientMock.Object,
                BaseUrl,
                QueueName);
        }

        [TestMethod]
        public async Task GetPricesForTodayAsync_ReturnsPriceSummary_WhenDataIsAvailable()
        {
            // Arrange
            var area = Area.SE1;
            var storedPrices = new Prices
            {
                PriceResults = new List<PriceResults>
                {
                    new PriceResults { SEK_per_kWh = 1.0, Area = area }
                }
            };
            _priceStorageMock.Setup(s => s.FetchAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(storedPrices);

            // Act
            var result = await _priceService.GetPricesForTodayAsync(area);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Prices.Count);
            Assert.AreEqual(1.0, result.AveragePrice);
        }

        [TestMethod]
        public async Task GetPricesForTodayAsync_FetchesFromApi_WhenStorageIsEmpty()
        {
            // Arrange
            var area = Area.SE1;
            _priceStorageMock.Setup(s => s.FetchAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((Prices)null);
            _priceApiFetcherMock.Setup(f => f.GetPricesForTodayAndArea(area, BaseUrl)).ReturnsAsync(new List<Price> { new Price { SEK_per_kWh = 1.0 } });

            // Act
            var result = await _priceService.GetPricesForTodayAsync(area);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Prices.Count);
            Assert.AreEqual(1.0, result.AveragePrice);
        }
    }
}
