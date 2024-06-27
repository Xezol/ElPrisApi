using Common.Interfaces;
using Common.Models;
using ElPrisApi.Controllers;
using ElPrisApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ElPrisApiTests
{
    [TestClass]
    public class ElPrisControllerTests
    {
        private Mock<ILogger<ElPrisController>> _loggerMock;
        private Mock<IPriceService> _priceServiceMock;
        private Mock<IWebHostEnvironment> _webHostEnvironmentMock;
        private GpsCoordinateChecker _gpsCoordinateChecker;
        private ElPrisController _controller;

        private PriceSummary PriceSummary = new PriceSummary
        {
            Prices = new List<Price> { new Price {  }
}
        };

        private Area Area = Area.SE1;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<ElPrisController>>();
            _priceServiceMock = new Mock<IPriceService>();
            _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            _webHostEnvironmentMock.Setup(env => env.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            // Create the GpsCoordinateChecker with the real GeoJSON file
            _gpsCoordinateChecker = new GpsCoordinateChecker(_webHostEnvironmentMock.Object);

            _controller = new ElPrisController(_loggerMock.Object, _priceServiceMock.Object, _gpsCoordinateChecker);

            _priceServiceMock.Setup(s => s.GetPricesForTodayAsync(Area)).ReturnsAsync(PriceSummary);
        }

        [TestMethod]
        public async Task GetPrices_ReturnsOk_WithPriceSummary()
        {
            // Act
            var result = await _controller.GetPrices(Area) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(PriceSummary, result.Value);
        }

        [TestMethod]
        public async Task GetPrices_ReturnsServerError_OnHttpRequestException()
        {
            // Arrange
            var area = Area.SE2;
            _priceServiceMock.Setup(s => s.GetPricesForTodayAsync(Area.SE2)).ThrowsAsync(new HttpRequestException("External service error"));

            // Act
            var result = await _controller.GetPrices(area) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(500, result.StatusCode);
            Assert.AreEqual("Error retrieving data from the external service: External service error", result.Value);
        }

        [TestMethod]
        public async Task GetPriceByGps_ReturnsOk_WithPriceSummary()
        {
            // Arrange
            double latitude = 66.56226014524397;
            double longitude = 15.544401275199407;

            // Act
            var result = await _controller.GetPriceByGps(latitude, longitude) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(PriceSummary, result.Value);
        }

        [TestMethod]
        public async Task GetPriceByGps_ReturnsBadRequest_WhenCoordinatesAreOutside()
        {
            // Arrange
            double latitude = 55.6761;
            double longitude = 12.5683;

            // Act
            var result = await _controller.GetPriceByGps(latitude, longitude) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("The provided GPS coordinates do not fall within any known area.", result.Value);
        }
    }
}
