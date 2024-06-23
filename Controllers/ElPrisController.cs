using ElPrisApi.Helpers;
using ElPrisApi.Interfaces;
using ElPrisApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ElPrisApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ElPrisController : ControllerBase
    {

        private readonly ILogger<ElPrisController> _logger;
        private readonly IPriceService _priceService;

        public ElPrisController(ILogger<ElPrisController> logger, IPriceService priceService)
        {
            _logger = logger;
            _priceService = priceService;
        }

        [HttpGet(Name = "Get Elpris by El område")]
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
    }
}
