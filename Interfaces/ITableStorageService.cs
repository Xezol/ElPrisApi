using ElPrisApi.Models;

namespace ElPrisApi.Interfaces
{
    public interface ITableStorageService
    {
        Task<List<PriceEntity>> GetPriceEntitiesAsync(string partitionKey);
        Task UpsertPriceEntitiesAsync(List<PriceEntity> entities);
    }
}
