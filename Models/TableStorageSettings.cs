namespace ElPrisApi.Models
{
    public class TableStorageSettings
    {
        public string StorageConnectionString { get; set; } = "UseDevelopmentStorage=true";
        public string TableName { get; set; } = "Prices";
    }
}
