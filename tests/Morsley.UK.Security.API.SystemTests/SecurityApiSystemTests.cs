using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;

namespace Morsley.UK.Security.API.SystemTests;

[TestFixture]
public class SecurityApiSystemTests
{
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    [OneTimeSetUp]
    public void Setup()
    {
        // Create a WebApplicationFactory to host the API with Keycloak configuration
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    // Override the Keycloak authority URL to use the test container's URL
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Authentication:Authority"] = $"{KeycloakFixture.KeycloakUrl}/realms/master",
                        ["Authentication:Audience"] = "account",
                        ["Authentication:RequireHttpsMetadata"] = "false"
                    });
                });
            });
        
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void Teardown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task Health_Endpoint_ShouldReturn_Success()
    {
        // Arrange - Keycloak is available via KeycloakFixture
        var keycloakUrl = KeycloakFixture.KeycloakUrl;
        
        keycloakUrl.ShouldNotBeEmpty("Keycloak should be running");
        
        // Act - Call the health endpoint (no auth required)
        var response = await _client!.GetAsync("/health");
        
        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Healthy");
    }

    [Test]
    public async Task WeatherForecast_WithoutToken_ShouldReturn_Unauthorized()
    {
        // Act - Call protected endpoint without auth token
        var response = await _client!.GetAsync("/weatherforecast");
        
        // Assert - Should be unauthorized
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task WeatherForecast_WithToken_ShouldReturn_OK()
    {
        // Arrange - Get a real token from Keycloak
        var tokenHelper = new KeycloakTokenHelper(KeycloakFixture.KeycloakUrl);
        
        var accessToken = await tokenHelper.GetAccessTokenAsync(
            KeycloakFixture.TestClientId,
            KeycloakFixture.TestClientSecret,
            KeycloakFixture.TestUsername,
            KeycloakFixture.TestUserPassword);

        accessToken.ShouldNotBeEmpty("Should have obtained an access token");

        // Create a new request with the Bearer token
        var request = new HttpRequestMessage(HttpMethod.Get, "/weatherforecast");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act - Call protected endpoint with auth token
        var response = await _client!.SendAsync(request);

        // Assert - Should succeed
        response.StatusCode.ShouldBe(HttpStatusCode.OK, 
            $"Expected OK but got {response.StatusCode}. Response: {await response.Content.ReadAsStringAsync()}");

        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeEmpty();
        Console.WriteLine($"Weather forecast response: {content}");
    }

    //[Test]
    //public void Keycloak_ShouldBe_Running()
    //{
    //    // Verify Keycloak container is accessible
    //    var keycloakUrl = KeycloakFixture.KeycloakUrl;
    //    var adminUsername = KeycloakFixture.AdminUsername;
    //    var adminPassword = KeycloakFixture.AdminPassword;

    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(keycloakUrl, Is.Not.Empty);
    //        Assert.That(adminUsername, Is.EqualTo("admin"));
    //        Assert.That(adminPassword, Is.EqualTo("admin"));
    //    });

    //    Console.WriteLine($"Keycloak is running at: {keycloakUrl}");
    //}

    [Test]
    public async Task Keycloak_HealthEndpoint_ShouldBe_Accessible()
    {
        // Arrange
        var keycloakUrl = KeycloakFixture.KeycloakUrl;
        using var httpClient = new HttpClient();

        // Act
        var response = await httpClient.GetAsync($"{keycloakUrl}health/ready");

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue($"Keycloak health check failed. Status: {response.StatusCode}");
    }
}
