using ElPrisApi.Services;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace ElPrisApiTests
{

    [TestClass]
    public class GpsCoordinateCheckerTests
    {
        private Mock<IWebHostEnvironment> _webHostEnvironmentMock;
        private GpsCoordinateChecker _gpsCoordinateChecker;

        [TestInitialize]
        public void Setup()
        {
            _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            _webHostEnvironmentMock.Setup(env => env.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            _gpsCoordinateChecker = new GpsCoordinateChecker(_webHostEnvironmentMock.Object);
        }

        [TestMethod]
        public void CheckPoint_ReturnsTrue_WhenPointIsInsidePolygon()
        {
            // Arrange
            double latitude = 66.56226014524397;
            double longitude = 15.544401275199407;

            // Act
            var result = _gpsCoordinateChecker.CheckPoint(latitude, longitude);

            // Assert
            Assert.IsTrue(result.inside);
            Assert.IsNotNull(result.area);
        }

        [TestMethod]
        public void CheckPoint_ReturnsFalse_WhenPointIsOutsidePolygon()
        {
            // Arrange
            double latitude = 55.6761;
            double longitude = 12.5683;

            // Act
            var result = _gpsCoordinateChecker.CheckPoint(latitude, longitude);

            // Assert
            Assert.IsFalse(result.inside);
            Assert.IsNull(result.area);
        }
    }

}
