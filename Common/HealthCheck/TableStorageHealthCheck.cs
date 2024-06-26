using Azure.Data.Tables;
using Common.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Common.HealthCheck
{
    public class TableStorageHealthCheck : IHealthCheck
    {
        private readonly ITableBlobStorageFor<TableEntity> _tableBlobStorageFor;

        public TableStorageHealthCheck(ITableBlobStorageFor<TableEntity> tableBlobStorageFor)
        {
            _tableBlobStorageFor = tableBlobStorageFor ?? throw new ArgumentNullException(nameof(tableBlobStorageFor));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Perform a simple query to check the health of the Table Storage
                var result = await _tableBlobStorageFor.ListAsync("healthcheck", 1);
                if (result != null)
                {
                    return HealthCheckResult.Healthy("Table Storage is healthy");
                }
                else
                {
                    return HealthCheckResult.Unhealthy("Table storage health check failed: no results returned");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Table storage health check failed: {ex.Message}");
            }
        }
    }
}
