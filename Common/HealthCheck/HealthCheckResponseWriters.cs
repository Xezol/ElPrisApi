using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace Common.HealthCheck
{
    public static class HealthCheckResponseWriters
    {
        public static Task WriteJsonResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";
            var response = new
            {
                status = report.Status.ToString(),
                results = report.Entries.ToDictionary(entry => entry.Key, entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    data = entry.Value.Data
                })
            };
            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }

}
