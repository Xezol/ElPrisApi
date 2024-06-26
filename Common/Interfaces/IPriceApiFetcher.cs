using Common.Models;

namespace Common.Interfaces
{
    public interface IPriceApiFetcher
    {
        Task<List<Price>> GetPricesForTodayAndArea(Area area, string baseUrl);
    }
}
