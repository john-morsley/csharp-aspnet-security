using Testcontainers.Keycloak;

namespace Morsley.UK.Security.API.SystemTests;

[SetUpFixture]
public class KeycloakFixture
{
    private static KeycloakContainer? _keycloakContainer;

    public static string KeycloakUrl { get; private set; } = string.Empty;
    public static string AdminUsername { get; private set; } = string.Empty;
    public static string AdminPassword { get; private set; } = string.Empty;
    
    // Test client credentials
    public const string TestClientId = "test-client";
    public const string TestClientSecret = "test-secret";
    public const string TestUsername = "testuser";
    public const string TestUserPassword = "testpass";

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        // Create and start Keycloak container
        _keycloakContainer = new KeycloakBuilder()
            .WithImage("quay.io/keycloak/keycloak:23.0")
            .WithUsername("admin")
            .WithPassword("admin")
            .Build();

        await _keycloakContainer.StartAsync();

        // Store connection details for tests
        KeycloakUrl = _keycloakContainer.GetBaseAddress();
        AdminUsername = "admin";
        AdminPassword = "admin";

        Console.WriteLine($"Keycloak started at: {KeycloakUrl}");

        // Set up test client and user
        await SetupKeycloakForTestsAsync();
    }

    private async Task SetupKeycloakForTestsAsync()
    {
        try
        {
            var setupHelper = new KeycloakSetupHelper(KeycloakUrl, AdminUsername, AdminPassword);

            // Wait a bit for Keycloak to be fully ready
            await Task.Delay(5000);

            // Create test client
            await setupHelper.CreateTestClientAsync(TestClientId, TestClientSecret);

            // Create test user
            await setupHelper.CreateTestUserAsync(TestUsername, TestUserPassword);

            Console.WriteLine("Keycloak setup complete with test client and user");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to setup Keycloak: {ex.Message}");
            Console.WriteLine("Tests requiring authentication may fail");
        }
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        if (_keycloakContainer != null)
        {
            await _keycloakContainer.StopAsync();
            await _keycloakContainer.DisposeAsync();
        }
    }
}
