using Microsoft.Toolkit.Uwp.Notifications;

namespace UqamAppWorkerService;


public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;
    private readonly UqamApiService _uqamAppService;

    public Worker(
        ILogger<Worker> logger, 
        IConfiguration configuration, 
        HttpClient httpClient,
        AuthService authService,
        UqamApiService uqamApiService
    )
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        _authService = authService;
        _uqamAppService = uqamApiService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while ( !stoppingToken.IsCancellationRequested)
        {
            var permanentCode = _configuration.GetValue<string>("PermanentCode");
            var password = _configuration.GetValue<string>("Password");
            ArgumentException.ThrowIfNullOrEmpty(permanentCode);
            ArgumentException.ThrowIfNullOrEmpty(password);
            
            var user = new User
            {
                Identifiant = permanentCode,
                MotDePasse = password,
            };

            bool hasNotification = await _uqamAppService.HasNotificationAsync();
            Console.WriteLine($"Has notification?: {hasNotification}");
            if ( hasNotification)
            {
#if WINDOWS
                var notifications = await _uqamAppService.GetNotificationsAsync(); 
                Console.WriteLine("Windows");
                new ToastContentBuilder()
                    .AddText(notifications.ToJsonString())
                    .Show();
#elif LINUX
                // TODO: Write the linux code here to handle the notification sent to the os
                Console.WriteLine("Linux");
#else
                Console.WriteLine("Multiple");
#endif
            }
            await Task.Delay(TimeSpan.FromSeconds(5));
        }        
    }
}
