using Azure.Data.Tables;

namespace ElPrisApi.Interfaces
{
    public interface ITableStorageService<T> where T : class, ITableEntity, new()
    {
        Task<List<T>> GetEntitiesAsync(string partitionKey);

        Task UpsertEntitiesAsync(List<T> entities);

        Task DeleteEntityAsync(string partitionKey, string rowKey);
    }
}
