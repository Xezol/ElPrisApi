using ElPrisApi;
using ElPrisApi.Interfaces;
using ElPrisApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddHttpClient<IPriceService, PriceService>();
builder.Services.AddSingleton<ITableStorageService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    string storageConnectionString = configuration.GetConnectionString("StorageConnectionString") ?? "UseDevelopmentStorage=true";
    string tableName = "Prices";
    return new TableStorageService(storageConnectionString, tableName);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
