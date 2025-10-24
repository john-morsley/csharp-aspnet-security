using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Morsley.UK.Security.API.SystemTests;

public class KeycloakSetupHelper
{
    private readonly string _keycloakUrl;
    private readonly string _adminUsername;
    private readonly string _adminPassword;
    private readonly HttpClient _httpClient;

    public KeycloakSetupHelper(string keycloakUrl, string adminUsername, string adminPassword)
    {
        _keycloakUrl = keycloakUrl.TrimEnd('/');
        _adminUsername = adminUsername;
        _adminPassword = adminPassword;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Get admin access token
    /// </summary>
    private async Task<string> GetAdminTokenAsync()
    {
        var tokenEndpoint = $"{_keycloakUrl}/realms/master/protocol/openid-connect/token";

        var requestData = new Dictionary<string, string>
        {
            { "client_id", "admin-cli" },
            { "username", _adminUsername },
            { "password", _adminPassword },
            { "grant_type", "password" }
        };

        var response = await _httpClient.PostAsync(
            tokenEndpoint,
            new FormUrlEncodedContent(requestData));

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return tokenResponse?.AccessToken ?? throw new Exception("Failed to get admin token");
    }

    /// <summary>
    /// Create a client in the master realm for testing
    /// </summary>
    public async Task<string> CreateTestClientAsync(string clientId, string clientSecret)
    {
        var adminToken = await GetAdminTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var clientEndpoint = $"{_keycloakUrl}/admin/realms/master/clients";

        var clientConfig = new
        {
            clientId = clientId,
            secret = clientSecret,
            enabled = true,
            directAccessGrantsEnabled = true,
            serviceAccountsEnabled = true,
            publicClient = false,
            protocol = "openid-connect",
            standardFlowEnabled = true,
            implicitFlowEnabled = false,
            redirectUris = new[] { "*" },
            webOrigins = new[] { "*" },
            attributes = new Dictionary<string, string>
            {
                { "access.token.lifespan", "3600" }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(clientEndpoint, clientConfig);

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            Console.WriteLine($"Client '{clientId}' already exists");
            return clientId;
        }

        response.EnsureSuccessStatusCode();
        Console.WriteLine($"Created client '{clientId}' successfully");
        return clientId;
    }

    /// <summary>
    /// Create a test user in the master realm
    /// </summary>
    public async Task CreateTestUserAsync(string username, string password)
    {
        var adminToken = await GetAdminTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var usersEndpoint = $"{_keycloakUrl}/admin/realms/master/users";

        var userConfig = new
        {
            username = username,
            enabled = true,
            credentials = new[]
            {
                new
                {
                    type = "password",
                    value = password,
                    temporary = false
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(usersEndpoint, userConfig);

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            Console.WriteLine($"User '{username}' already exists");
            return;
        }

        response.EnsureSuccessStatusCode();
        Console.WriteLine($"Created user '{username}' successfully");
    }

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
    }
}
