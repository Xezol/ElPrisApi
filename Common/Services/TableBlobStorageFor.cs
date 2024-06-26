using Azure;
using Azure.Data.Tables;
using Common.Interfaces;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Services
{
    public class TableBlobStorageFor<TModel> : ITableBlobStorageFor<TModel>
    {
        private static readonly string ColumnsCountColumnName = "__columnsSpanned";
        private static readonly string DataLengthColumnName = "__dataLength";

        private static readonly string ColumnNamePrefix = "__e";
        private static readonly int StorageTableSingleCellSize = 32 * 1024;
        private static readonly int MaxSupportedColumns = 25;

        private readonly TableClient _tableClient;

        public TableBlobStorageFor(string tableName, string connStr)
        {
            _tableClient = new TableClient(connStr, tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task<List<TModel>> GetAllAsync()
        {
            var returnValue = new List<TModel>();
            var queryResults = _tableClient.QueryAsync<TableEntity>();

            foreach (var item in await queryResults.ToListAsync())
            {
                returnValue.Add(ExtractModelFromDynamicTableEntity(item));
            }

            return returnValue;
        }

        public async Task<List<TModel>> ListAsync(string partitionKey, int limit = 1)
        {
            var returnValue = new List<TModel>();
            var queryResults = _tableClient.QueryAsync<TableEntity>(x => x.PartitionKey == partitionKey, maxPerPage: limit);

            foreach (var item in await queryResults.ToListAsync())
            {
                returnValue.Add(ExtractModelFromDynamicTableEntity(item));
            }

            return returnValue;
        }

        public Task<TModel> FetchAsync(Guid partitionKey, string rowKey = "1000")
        {
            return FetchAsync(partitionKey.ToString(), rowKey);
        }


        public async Task DeleteAsync(Guid partitionKey, string rowKey = "1000")
        {
            await _tableClient.DeleteEntityAsync(partitionKey.ToString(), rowKey);
        }

        public async Task DeleteManyAsync(List<Guid> partitionKeys, string rowKey = "1000")
        {
            foreach (var partitionKey in partitionKeys)
            {
                await _tableClient.DeleteEntityAsync(partitionKey.ToString(), rowKey);
            }
        }

        public async Task DeleteAllByPartitionKeyAsync(string partitionKey, int limit = 1)
        {
            var queryResults = _tableClient.QueryAsync<TableEntity>(x => x.PartitionKey == partitionKey, maxPerPage: limit);

            foreach (var item in await queryResults.ToListAsync())
            {
                await _tableClient.DeleteEntityAsync(partitionKey, item.RowKey);
            }
        }

        public async Task<TModel> FetchAsync(string partitionKey, string rowKey = "1000")
        {
            try
            {
                var retrievedResult = await _tableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey).ConfigureAwait(false);
                return ExtractModelFromDynamicTableEntity(retrievedResult);
            }
            catch (RequestFailedException azex)
            {
                if (azex.Status == (int)HttpStatusCode.NotFound)
                {
                    return default;
                }
                else
                {
                    throw;
                }
            }
        }

        public Task SaveAsync(TModel data, Guid partitionKey, string rowKey = "1000")
        {
            return SaveAsync(data, partitionKey.ToString(), rowKey);
        }

        public async Task SaveAsync(TModel data, string partitionKey, string rowKey = "1000")
        {
            var tableEntity = new TableEntity(partitionKey, rowKey);
            var dataAsJson = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            byte[] bytes = Encoding.UTF8.GetBytes(dataAsJson);
            byte[] compressedData = bytes.Length == 0 ? new byte[0] : GzipByte(bytes);

            tableEntity.Add(DataLengthColumnName, compressedData.Length);

            int columnCount = 0;
            double colsNeeded = Math.Ceiling((double)compressedData.Length / StorageTableSingleCellSize);
            if (colsNeeded > MaxSupportedColumns)
            {
                throw new InvalidOperationException($"Storing of {compressedData.Length / 1024} exceeds 800KB (25 columns)");
            }

            for (int i = 0; i < colsNeeded; i++)
            {
                int lengthToFetch = i == colsNeeded - 1 ? compressedData.Length % StorageTableSingleCellSize : StorageTableSingleCellSize;
                tableEntity.Add(ColumnNamePrefix + (i + 1), compressedData.Skip(StorageTableSingleCellSize * i).Take(lengthToFetch).ToArray());
                columnCount++;
            }

            tableEntity.Add(ColumnsCountColumnName, columnCount);

            // Save
            await _tableClient.UpsertEntityAsync(tableEntity);
        }


        private static TModel ExtractModelFromDynamicTableEntity(TableEntity tableEntity)
        {
            if (!tableEntity.ContainsKey(ColumnsCountColumnName))
            {
                return default;
            }

            int dataLength = tableEntity.GetInt32(DataLengthColumnName) ?? 0;
            int columnCount = tableEntity.GetInt32(ColumnsCountColumnName) ?? 0;

            byte[] dataArray = new byte[dataLength];
            for (int i = 0; i < columnCount; i++)
            {
                tableEntity.GetBinary(ColumnNamePrefix + (i + 1)).CopyTo(dataArray, i * StorageTableSingleCellSize);
                tableEntity.Remove(ColumnNamePrefix + (i + 1));
            }

            return GunzipByte<TModel>(dataArray);
        }

        private static byte[] GzipByte(byte[] str)
        {
            if (str == null)
            {
                return null;
            }

            using (var output = new MemoryStream())
            {
                using (var compressor = new Ionic.Zlib.GZipStream(output, Ionic.Zlib.CompressionMode.Compress, Ionic.Zlib.CompressionLevel.BestCompression))
                {
                    compressor.Write(str, 0, str.Length);
                }
                return output.ToArray();
            }
        }

        private static TModel GunzipByte<T>(byte[] str)
        {
            if (str.Length == 0) return default;
            var decompressed = Ionic.Zlib.GZipStream.UncompressBuffer(str);
            string json = Encoding.UTF8.GetString(decompressed);
            return JsonSerializer.Deserialize<TModel>(json);
        }
    }
}
