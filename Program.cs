using UqamAppWorkerService;
using UqamAppWorkerService.Services;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    services.AddHostedService<Worker>();
    services.AddSingleton<HttpClient>();
    services.AddScoped<AuthService>();
    services.AddScoped<UqamApiService>();
});

var host = builder.Build();
await host.RunAsync();
