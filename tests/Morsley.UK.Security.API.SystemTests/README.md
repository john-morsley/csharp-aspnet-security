# Morsley.UK.Security.API.SystemTests

System tests for the Security API using NUnit and Testcontainers.

## Features

- **NUnit** test framework
- **Testcontainers.Keycloak** for OAuth2/OIDC testing
- **WebApplicationFactory** for in-memory API hosting
- Shared Keycloak container across all tests for performance

## Running Tests

```powershell
dotnet test
```

## Test Structure

- **KeycloakFixture.cs**: Global setup that starts a Keycloak container once for all tests
- **SecurityApiSystemTests.cs**: Sample system tests demonstrating API and Keycloak integration

## Keycloak Container

The Keycloak container is automatically started before any tests run and stopped after all tests complete. 

**Default credentials:**
- Username: `admin`
- Password: `admin`

Access Keycloak URL in tests via: `KeycloakFixture.KeycloakUrl`

## Adding New Tests

1. Create a new test class
2. Access Keycloak via the static properties in `KeycloakFixture`
3. Use `WebApplicationFactory<Program>` to test the API with OAuth2 integration
