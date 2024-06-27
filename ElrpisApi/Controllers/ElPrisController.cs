using Common.Interfaces;
using Common.Models;
using ElPrisApi.Helpers;
using ElPrisApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElPrisApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ElPrisController : ControllerBase
    {
        private readonly ILogger<ElPrisController> _logger;
        private readonly IPriceService _priceService;
        private readonly GpsCoordinateChecker _gpsCoordinateChecker;

        public ElPrisController(ILogger<ElPrisController> logger, IPriceService priceService, GpsCoordinateChecker gpsCoordinateChecker)
        {
            _logger = logger;
            _priceService = priceService;
            _gpsCoordinateChecker = gpsCoordinateChecker;
        }

        [HttpGet("GetElprisByArea", Name = "Get Elpris by El område")]
        public async Task<IActionResult> GetPrices([ModelBinder(BinderType = typeof(AreaBinder))] Area area)
        {
            try
            {
                var priceSummary = await _priceService.GetPricesForTodayAsync(area);
                if (priceSummary.Prices == null || priceSummary.Prices.Count == 0)
                {
                    return NotFound("No prices found for the specified date and price class.");
                }
                return Ok(priceSummary);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error retrieving data from the external service: {ex.Message}");
            }
        }

        [HttpGet("GetPriceByGps")]
        public async Task<IActionResult> GetPriceByGps(double latitude, double longitude)
        {
            var (inside, area) = _gpsCoordinateChecker.CheckPoint(latitude, longitude);

            if (inside && area.HasValue)
            {
                var priceSummary = await _priceService.GetPricesForTodayAsync(area.Value);

                if (priceSummary.Prices == null || priceSummary.Prices.Count == 0)
                {
                    return NotFound("No prices found for the specified date and area.");
                }

                return Ok(priceSummary);
            }

            return BadRequest("The provided GPS coordinates do not fall within any known area.");
        }
    }
}
