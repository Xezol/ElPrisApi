using ElPrisApi;
using ElPrisApi.Helpers;
using ElPrisApi.Interfaces;
using ElPrisApi.Models;
using ElPrisApi.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.Configure<TableStorageSettings>(builder.Configuration.GetSection("TableStorageSettings"));
builder.Services.AddHttpClient<IPriceService, PriceService>();

builder.Services.AddSingleton(typeof(ITableStorageService<>), typeof(TableStorageService<>));

// Register the custom health check for Table Storage
builder.Services.AddSingleton<TableStorageHealthCheck>();

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
app.MapHealthChecks("/healthy/table", new HealthCheckOptions
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
