using ElPrisApi.Interfaces;
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
        public async Task<IActionResult> GetPrices(string elOmrade)
        {
            try
            {
                var prices = await _priceService.GetPricesForTodayAsync(elOmrade);
                if (prices == null || prices.Count == 0)
                {
                    return NotFound("No prices found for the specified date and price class.");
                }
                return Ok(prices);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error retrieving data from the external service: {ex.Message}");
            }
        }
    }
}
