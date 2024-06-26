using Common.Models;

namespace Common.Interfaces
{
    public interface IPriceService
    {
        Task<PriceSummary> GetPricesForTodayAsync(Area area);
    }
}
