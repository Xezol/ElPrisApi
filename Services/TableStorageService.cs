using Azure.Data.Tables;
using ElPrisApi.Interfaces;
using ElPrisApi.Models;
using Microsoft.Extensions.Options;

namespace ElPrisApi.Services
{
    public class TableStorageService<T> : ITableStorageService<T> where T : class, ITableEntity, new()
    {
        private readonly TableClient _tableClient;

        public TableStorageService(IOptions<TableStorageSettings> settings)
        {
            var serviceClient = new TableServiceClient(settings.Value.StorageConnectionString);
            _tableClient = serviceClient.GetTableClient(settings.Value.TableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task<List<T>> GetEntitiesAsync(string partitionKey)
        {
            var query = _tableClient.QueryAsync<T>(entity => entity.PartitionKey.Equals(partitionKey, StringComparison.InvariantCultureIgnoreCase));
            var entities = new List<T>();

            await foreach (var entity in query)
            {
                entities.Add(entity);
            }

            return entities;
        }

        public async Task UpsertEntitiesAsync(List<T> entities)
        {
            foreach (var entity in entities)
            {
                await _tableClient.UpsertEntityAsync(entity);
            }
        }

        public async Task DeleteEntityAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public TableClient GetTableClient()
        {
            return _tableClient;
        }
    }
}
