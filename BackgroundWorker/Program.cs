using Common.Interfaces;
using Common.Models;
using Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<ITableBlobStorageFor<Prices>>(provider =>
                new TableBlobStorageFor<Prices>(nameof(Prices), Environment.GetEnvironmentVariable("AzureWebJobsStorage")));
    })
    .Build();

host.Run();
