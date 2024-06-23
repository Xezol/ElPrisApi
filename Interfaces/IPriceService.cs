using ElPrisApi.Models;

namespace ElPrisApi.Interfaces
{
    public interface IPriceService
    {
        Task<List<Price>> GetPricesForTodayAsync(string priceClass);
    }
}
