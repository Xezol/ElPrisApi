using ElPrisApi.Models;

namespace ElPrisApi.Interfaces
{
    public interface IPriceService
    {
        Task<PriceSummary> GetPricesForTodayAsync(Area area);
    }
}
