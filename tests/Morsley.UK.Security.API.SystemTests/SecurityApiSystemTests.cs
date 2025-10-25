namespace Morsley.UK.Security.API.SystemTests;

[TestFixture]
public class SecurityApiSystemTests
{
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    [OneTimeSetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {                    
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
    public async Task Keycloak_Health_Endpoint_ShouldBe_Accessible()
    {
        // Arrange
        var keycloakUrl = KeycloakFixture.KeycloakUrl;
        using var httpClient = new HttpClient();

        // Act
        var response = await httpClient.GetAsync($"{keycloakUrl}health");

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue($"Keycloak /health endpoint failed. Status: {response.StatusCode}");
        
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("\"status\"");
    }

    [Test]
    public async Task Keycloak_HealthReady_Endpoint_ShouldBe_Accessible()
    {
        // Arrange
        var keycloakUrl = KeycloakFixture.KeycloakUrl;
        using var httpClient = new HttpClient();

        // Act
        var response = await httpClient.GetAsync($"{keycloakUrl}health/ready");

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue($"Keycloak /health/ready endpoint failed. Status: {response.StatusCode}");
        
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("\"status\"");
    }

    [Test]
    public async Task Keycloak_HealthLive_Endpoint_ShouldBe_Accessible()
    {
        // Arrange
        var keycloakUrl = KeycloakFixture.KeycloakUrl;
        using var httpClient = new HttpClient();

        // Act
        var response = await httpClient.GetAsync($"{keycloakUrl}health/live");

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue($"Keycloak /health/live endpoint failed. Status: {response.StatusCode}");
        
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("\"status\"");
    }

    [Test]
    public async Task Keycloak_HealthStarted_Endpoint_ShouldBe_Accessible()
    {
        // Arrange
        var keycloakUrl = KeycloakFixture.KeycloakUrl;
        using var httpClient = new HttpClient();

        // Act
        var response = await httpClient.GetAsync($"{keycloakUrl}health/started");

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue($"Keycloak /health/started endpoint failed. Status: {response.StatusCode}");
        
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("\"status\"");
    }

    [Test]
    public async Task Health_Endpoint_ShouldReturn_Success()
    {
        // Arrange
        var keycloakUrl = KeycloakFixture.KeycloakUrl;
        
        keycloakUrl.ShouldNotBeEmpty("Keycloak should be running");
        
        // Act
        var response = await _client!.GetAsync("/health");
        
        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Healthy");
    }

    /// <summary>
    /// Tests that the API correctly rejects unauthenticated requests to protected endpoints.
    /// This test verifies that:
    /// 1. When a request is made to a protected endpoint (marked with [Authorize]) without any authentication token
    /// 2. The API's authentication middleware detects the missing Authorization header
    /// 3. The request is rejected before reaching the controller action
    /// 4. The API returns HTTP 401 Unauthorized status code
    /// 
    /// This is a critical security test that ensures:
    /// - Protected endpoints cannot be accessed anonymously
    /// - The [Authorize] attribute is properly enforced
    /// - The authentication middleware is correctly configured and active in the request pipeline
    /// - Unauthorized access attempts are blocked at the authentication layer
    /// 
    /// This test proves the "secure by default" principle - endpoints marked with [Authorize]
    /// are inaccessible without valid credentials, preventing unauthorized access to sensitive resources.
    /// This is the complement to the "WithToken" test, demonstrating that authentication is required.
    /// </summary>
    [Test]
    public async Task WeatherForecast_WithoutToken_ShouldReturn_Unauthorized()
    {
        // Act
        var response = await _client!.GetAsync("/greeting");
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Tests the complete OAuth 2.0 authentication flow using the Client Credentials grant type.
    /// This test verifies that:
    /// 1. The API can successfully obtain a JWT access token from Keycloak using client credentials (client ID and client secret)
    /// 2. The obtained token can be used to authenticate against a protected endpoint (marked with [Authorize])
    /// 3. The API's JWT Bearer authentication middleware correctly validates the token by:
    ///    - Verifying the token's signature using Keycloak's public keys
    ///    - Checking the token's issuer matches the configured Authority
    ///    - Validating the token's audience matches the configured value
    ///    - Ensuring the token hasn't expired
    /// 4. When a valid token is provided in the Authorization header, the API returns HTTP 200 OK
    /// 5. The protected endpoint returns the expected response content ("Hello")
    /// 
    /// This is an end-to-end integration test that exercises the entire authentication chain:
    /// - Keycloak (running in Docker via Testcontainers) issues the token
    /// - The test client includes the token in the Authorization: Bearer header
    /// - The API's authentication middleware validates the token
    /// - The protected controller action executes and returns a response
    /// 
    /// This test proves that the OAuth 2.0 security configuration is working correctly and that
    /// legitimate clients with valid credentials can successfully access protected API resources.
    /// </summary>
    [Test]
    public async Task WeatherForecast_WithToken_ShouldReturn_OK()
    {
        // Arrange
        var tokenHelper = new KeycloakTokenHelper(KeycloakFixture.KeycloakUrl);
        
        var accessToken = await tokenHelper.GetClientCredentialsTokenAsync(
            KeycloakFixture.TestClientId,
            KeycloakFixture.TestClientSecret);

        accessToken.ShouldNotBeEmpty("Should have obtained an access token");

        var request = new HttpRequestMessage(HttpMethod.Get, "/greeting");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await _client!.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeEmpty();
        content.ShouldBe("Hello");
    }
}