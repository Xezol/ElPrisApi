using Azure.Data.Tables;
using ElPrisApi.Interfaces;
using ElPrisApi.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class TableStorageHealthCheck : IHealthCheck
{
    private readonly ITableStorageService<TableEntity> _tableStorageService;

    public TableStorageHealthCheck(ITableStorageService<TableEntity> tableStorageService)
    {
        _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var tableClient = ((TableStorageService<TableEntity>)_tableStorageService).GetTableClient();
            var x = tableClient.QueryAsync<TableEntity>(p => 1 == 1, maxPerPage: 1);

            // Perform a simple query to check the health of the Table Storage
            await foreach (var _ in x)
            {
                // Just iterating to check connectivity
            }
            return HealthCheckResult.Healthy("Table Storage is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Table storage health check failed: {ex.Message}");
        }
    }
}