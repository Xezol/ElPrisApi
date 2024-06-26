using Azure;
using Azure.Data.Tables;


namespace BackgroundWorker.Models
{
    public class AzureTableStorage
    {
        private readonly TableClient _tableClient;

        public AzureTableStorage(string connectionString, string tableName)
        {
            TableServiceClient serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task DeleteRowsByPartitionKeyAsync(DateTime date)
        {
            string partitionKey = date.ToShortDateString();
            string rowKey = "1000";

            try
            {
                // Fetch the entity
                var entity = await _tableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey);

                // Delete the entity
                await _tableClient.DeleteEntityAsync(partitionKey, rowKey);

                Console.WriteLine($"Deleted entity with PartitionKey: {partitionKey}, RowKey: {rowKey}");
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                Console.WriteLine($"Entity with PartitionKey: {partitionKey}, RowKey: {rowKey} not found.");
            }
        }
    }

}
