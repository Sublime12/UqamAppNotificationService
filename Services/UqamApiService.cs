using System.Security.Authentication;
using System.Text.Json;
using System.Text.Json.Nodes;

using UqamAppWorkerService.Models;
using UqamAppWorkerService.Services;

namespace UqamAppWorkerService;

public class UqamApiService
{
    private readonly AuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<UqamApiService> _logger;
    private string? _token;

    private static readonly string HOST = "https://monportail.uqam.ca";
    private static readonly string UQAM_NOTIFICATION_URL = $"{HOST}/apis/notifications/obtenirNotifications";
    private static readonly string UQAM_IDENTIFIANT_URL = $"{HOST}/apis/resume/identifiant";
    private static readonly string UQAM_RESUME_TRIMESTRES_URL = $"{HOST}/apis/resumeResultat/identifiant";
    private static readonly string UQAM_ACTIVITE_URL = $"{HOST}/apis/resultatActivite/identifiant";

    public UqamApiService(AuthService authService, IConfiguration configuration, HttpClient httpClient, ILogger<UqamApiService> logger)
    {
        _authService = authService;
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetTokenAsync()
    {
        if (_token is not null)
        {
            return _token;
        }

        var loginResponse = await _authService.LoginAsync(new User
        {
            Identifiant = _configuration.GetValue<string>("PermanentCode")!,
            MotDePasse = _configuration.GetValue<string>("Password")!,
        });

        if (loginResponse is null)
        {
            throw new AuthenticationException("Can't connect to the uqam portal");
        }
        
        _token = loginResponse.Token;

        return _token;
    }

    public async Task<bool> HasNotificationAsync()
    {
        var notificationsNode = await GetNotificationsAsync(); 
        var countNotification = notificationsNode["data"]!["notifications"]!.AsArray().Count();

        return countNotification != 0;
    }

    public async Task<JsonNode> GetNotificationsAsync()
    {
        
        var requestMessage = new HttpRequestMessage();
        requestMessage.Headers.Add("Authorization", $"Bearer {await GetTokenAsync()}");
        requestMessage.Method = HttpMethod.Get;
        requestMessage.RequestUri = new Uri(UQAM_NOTIFICATION_URL);

        var response = await _httpClient.SendAsync(requestMessage);

        var notificationsNode = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        return notificationsNode;
    }

    public async Task<IEnumerable<Trimestre>> GetTrimestresAsync()
    {
        var httpRequestMessage = new HttpRequestMessage();
        httpRequestMessage.Headers.Add("Authorization", $"Bearer {await GetTokenAsync()}");
        httpRequestMessage.Method = HttpMethod.Get;
        httpRequestMessage.RequestUri = new Uri(UQAM_IDENTIFIANT_URL);
        var response = await _httpClient.SendAsync(httpRequestMessage);
        string json = await response.Content.ReadAsStringAsync();
        var identifiantNode = JsonNode.Parse(json);
        var trimestresNode = identifiantNode["data"]["resume"]["trimestresCourants"];

        var trimestres = trimestresNode.AsArray().Deserialize<List<Trimestre>>();

        return trimestres;
    }

    public async Task<List<TrimestreAvecProgrammes>> GetResumeTrimestresAvecActivitesAsync()
    {
        var httpRequestMessage = new HttpRequestMessage();
        httpRequestMessage.Headers.Add("Authorization", $"Bearer {await GetTokenAsync()}");
        httpRequestMessage.Method = HttpMethod.Get;
        httpRequestMessage.RequestUri = new Uri(UQAM_RESUME_TRIMESTRES_URL);
        var response = await _httpClient.SendAsync(httpRequestMessage);
        string json = await response.Content.ReadAsStringAsync();
        
        var resultatNode = JsonNode.Parse(json);
        var trimestreAvecProgrammeNode = resultatNode["data"]["resultats"];

        var trimestresAvecProgrammes = trimestreAvecProgrammeNode.Deserialize<List<TrimestreAvecProgrammes>>();
    
        return trimestresAvecProgrammes;
    }

    public async Task<List<Evaluation>> GetEvaluationForAsync(Activite activite)
    {
        var httpRequestMessage = new HttpRequestMessage();
        httpRequestMessage.Headers.Add("Authorization", $"Bearer {GetTokenAsync().Result}");
        httpRequestMessage.Method = HttpMethod.Get;
        httpRequestMessage.RequestUri = new Uri($"{UQAM_ACTIVITE_URL}/{activite.Trimestre}/{activite.Sigle}/{activite.Groupe}");
        var response = await _httpClient.SendAsync(httpRequestMessage);
        string json = await response.Content.ReadAsStringAsync();
        var resultNode = JsonNode.Parse(json);

        var evaluationsNode = resultNode["data"]["resultats"][0]["programmes"][0]["activites"][0]["evaluations"];

        return evaluationsNode is null ? 
            new List<Evaluation>(0) : 
            evaluationsNode.Deserialize<List<Evaluation>>();
    }

}
