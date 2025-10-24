using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Morsley.UK.Security.API.SystemTests;

public class KeycloakTokenHelper
{
    private readonly string _keycloakUrl;
    private readonly string _realm;
    private readonly HttpClient _httpClient;

    public KeycloakTokenHelper(string keycloakUrl, string realm = "master")
    {
        _keycloakUrl = keycloakUrl.TrimEnd('/');
        _realm = realm;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Get access token using Resource Owner Password Credentials flow
    /// </summary>
    public async Task<string> GetAccessTokenAsync(
        string clientId,
        string clientSecret,
        string username,
        string password)
    {
        var tokenEndpoint = $"{_keycloakUrl}/realms/{_realm}/protocol/openid-connect/token";

        var requestData = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "username", username },
            { "password", password },
            { "grant_type", "password" }
        };

        var response = await _httpClient.PostAsync(
            tokenEndpoint,
            new FormUrlEncodedContent(requestData));

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        
        return tokenResponse?.AccessToken ?? throw new Exception("Failed to get access token");
    }

    /// <summary>
    /// Get access token using Client Credentials flow (no user required)
    /// </summary>
    public async Task<string> GetClientCredentialsTokenAsync(
        string clientId,
        string clientSecret)
    {
        var tokenEndpoint = $"{_keycloakUrl}/realms/{_realm}/protocol/openid-connect/token";

        var requestData = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "grant_type", "client_credentials" }
        };

        var response = await _httpClient.PostAsync(
            tokenEndpoint,
            new FormUrlEncodedContent(requestData));

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        
        return tokenResponse?.AccessToken ?? throw new Exception("Failed to get access token");
    }

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
    }
}
