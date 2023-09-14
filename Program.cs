using UqamAppWorkerService;
using UqamAppWorkerService.Models;
using UqamAppWorkerService.Services;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    services.AddHostedService<Worker>();
    services.AddSingleton<HttpClient>();
    services.AddScoped<AuthService>();
    services.AddScoped<UqamApiService>();
    services.AddScoped<IOldTrimestreTookService, OldTrimestreTookSaveInJsonFileService>();
});

var host = builder.Build();
await host.RunAsync();
