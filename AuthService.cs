

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

public class AuthService
{
    private HttpClient _httpClient;

    public const string LOGIN_URL = "https://monportail.uqam.ca/authentification";

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Try to login to the uqam portal
    /// </summary>
    /// <param name="user"></param>
    /// <exception cref="JsonException">throw if the methode can't deserialize to
    /// a LoginResponse</exception>
    /// <returns>LoginResponse representing the json received from the server</returns>
    public async Task<LoginResponse?> LoginAsync(User user)
    {
        var jsonContent = JsonContent.Create(new Dictionary<string, object>()
        {
            {"identifiant", user.Identifiant},
            {"motDePasse", user.MotDePasse},
        });

        var response = await _httpClient.PostAsync(LOGIN_URL, jsonContent);

        var responseAsString = await response.Content.ReadAsStringAsync();
        LoginResponse? loginResponse = null;

        loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseAsString, 
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        });
    
        return loginResponse;
    }  
}

public class LoginResponse
{
    public required string Token { get; set; }

    public required Utilisateur Utilisateur { get; set; }
}

public class Utilisateur
{
    public required Socio Socio { get; set; }

    public required string Role;
}

public class Socio
{
    [JsonPropertyName("code_perm")]
    public required string CodePerm { get; set; }

    public required string Nom { get; set; }

    public required string Prenom { get; set; }

}

public class User
{
    [JsonPropertyName("identifiant")]
    public required string Identifiant;

    [JsonPropertyName("motDePasse")]
    public required string MotDePasse;

}