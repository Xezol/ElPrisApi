using Azure.Data.Tables;
using ElPrisApi.Interfaces;
using ElPrisApi.Models;

namespace ElPrisApi.Services
{
    public class TableStorageService : ITableStorageService
    {
        private readonly TableClient _tableClient;

        public TableStorageService(string storageConnectionString, string tableName)
        {
            var serviceClient = new TableServiceClient(storageConnectionString);
            _tableClient = serviceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task<List<PriceEntity>> GetPriceEntitiesAsync(string partitionKey)
        {
            var query = _tableClient.QueryAsync<PriceEntity>(entity => entity.PartitionKey == partitionKey);
            var entities = new List<PriceEntity>();

            await foreach (var entity in query)
            {
                entities.Add(entity);
            }

            return entities;
        }

        public async Task UpsertPriceEntitiesAsync(List<PriceEntity> entities)
        {
            foreach (var entity in entities)
            {
                await _tableClient.UpsertEntityAsync(entity);
            }
        }
    }
}