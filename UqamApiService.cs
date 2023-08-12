using System.Security.Authentication;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UqamAppWorkerService;

public class UqamApiService
{
    private readonly AuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<UqamApiService> _logger;
    private readonly string UQAM_NOTIFICATION_URL = "https://monportail.uqam.ca/apis/notifications/obtenirNotifications";

    public UqamApiService(AuthService authService, IConfiguration configuration, HttpClient httpClient, ILogger<UqamApiService> logger)
    {
        _authService = authService;
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> HasNotificationAsync()
    {
        var notificationsNode = await GetNotificationsAsync(); 
        // var notificationsNode = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
        var countNotification = notificationsNode["data"]!["notifications"]!.AsArray().Count();

        _logger.LogInformation($"Nb of Document: {countNotification}");
        return countNotification != 0;
    }

    public async Task<JsonNode> GetNotificationsAsync()
    {
        var loginResponse = await _authService.LoginAsync(new User
        {
            Identifiant = _configuration.GetValue<string>("PermanentCode")!,
            MotDePasse = _configuration.GetValue<string>("Password")!,
        });

        if (loginResponse is null)
        {
            throw new AuthenticationException("Can't connect to the uqam portal");
        }
        var requestMessage = new HttpRequestMessage();
        requestMessage.Headers.Add("Authorization", $"Bearer {loginResponse.Token}");
        requestMessage.Method = HttpMethod.Get;
        requestMessage.RequestUri = new Uri(UQAM_NOTIFICATION_URL);

        var response = await _httpClient.SendAsync(requestMessage);

        _logger.LogInformation(await response.Content.ReadAsStringAsync());
        var notificationsNode = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        return notificationsNode;
        // return response;
    }

}