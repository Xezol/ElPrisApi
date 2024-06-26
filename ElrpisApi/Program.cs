using Azure.Messaging.ServiceBus;
using Common.HealthCheck;
using Common.Interfaces;
using Common.Models;
using Common.Services;
using ElPrisApi;
using ElPrisApi.Helpers;
using ElPrisApi.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddHttpClient<IPriceApiFetcher, PriceApiFetcher>();

builder.Services.AddSingleton<ServiceBusClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new ServiceBusClient(configuration.GetConnectionString("ServiceBusConnectionString"));
});
builder.Services.AddSingleton<IPriceService, PriceService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var priceApiFetcher = provider.GetRequiredService<IPriceApiFetcher>();
    var priceStorage = provider.GetRequiredService<ITableBlobStorageFor<Prices>>();
    var serviceBusClient = provider.GetRequiredService<ServiceBusClient>();
    string baseUrl = configuration["ApiSettings:BaseUrl"];
    string queueName = configuration["ServiceBusSettings:QueueName"];
    return new PriceService(priceApiFetcher, priceStorage, serviceBusClient, baseUrl, queueName);
});

builder.Services.AddSingleton<ITableBlobStorageFor<Prices>>(provider =>
    new TableBlobStorageFor<Prices>(nameof(Prices), provider.GetRequiredService<IConfiguration>().GetConnectionString("StorageConnectionString") ?? "UseDevelopmentStorage=true"));


builder.Services.AddSingleton<GpsCoordinateChecker>();

// Add health checks
string? baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
if (string.IsNullOrEmpty(baseUrl))
{
    throw new ArgumentNullException("ApiSettings:BaseUrl", "Base URL for underlying API cannot be null or empty.");
}

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Healthy"))
    .AddCheck<TableStorageHealthCheck>("TableStorage")
    .AddUrlGroup(new Uri(baseUrl), name: "Underlying API");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Price API", Version = "v1" });
    c.SchemaFilter<EnumSchemaFilter>(); // Use the custom schema filter for enums
    c.MapType<Area>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames(typeof(Area)).Select(name => new OpenApiString(name)).ToList<IOpenApiAny>()
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Price API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Map health check endpoints
app.MapHealthChecks("/healthy", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriters.WriteJsonResponse
});
app.MapHealthChecks("/healthy/self", new HealthCheckOptions
{
    Predicate = check => check.Name == "self",
    ResponseWriter = HealthCheckResponseWriters.WriteJsonResponse
});
app.MapHealthChecks("/healthy/tablestorage", new HealthCheckOptions
{
    Predicate = check => check.Name == "TableStorage",
    ResponseWriter = HealthCheckResponseWriters.WriteJsonResponse
});
app.MapHealthChecks("/healthy/underlying-api", new HealthCheckOptions
{
    Predicate = check => check.Name == "Underlying API",
    ResponseWriter = HealthCheckResponseWriters.WriteJsonResponse
});

app.Run();
