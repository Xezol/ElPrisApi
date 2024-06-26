namespace Common.Interfaces
{
    public interface ITableBlobStorageFor<TModel>
    {
        Task SaveAsync(TModel data, string partitionKey, string rowKey = "1000");

        Task SaveAsync(TModel data, Guid partitionKey, string rowKey = "1000");

        Task<TModel> FetchAsync(string partitionKey, string rowKey = "1000");

        Task<TModel> FetchAsync(Guid partitionKey, string rowKey = "1000");

        Task<List<TModel>> GetAllAsync();

        Task<List<TModel>> ListAsync(string partitionKey, int limit = 1);

        Task DeleteAsync(Guid partitionKey, string rowKey = "1000");

        Task DeleteManyAsync(List<Guid> partitionKeys, string rowKey = "1000");

        Task DeleteAllByPartitionKeyAsync(string partitionKey, int limit = 1);

    }
}
