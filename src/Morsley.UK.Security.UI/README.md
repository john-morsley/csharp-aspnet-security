# Morsley.UK.Security.UI

A web UI for demonstrating OAuth 2.0 authentication with Keycloak and a secured ASP.NET Core API.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Docker Compose](https://docs.docker.com/compose/install/) (included with Docker Desktop)

## Getting Started

### 1. Start Keycloak

From the **solution root directory** (where `docker-compose.yaml` is located):

```bash
# Start Keycloak in detached mode
docker-compose up -d

# View logs to confirm Keycloak is running
docker-compose logs -f keycloak

# Wait for this message:
# "Keycloak X.X.X on JVM (powered by Quarkus X.X.X) started in X.XXXs"
```

**Keycloak will be available at:** `http://localhost:8080`

**Admin Console:** `http://localhost:8080/admin`
- Username: `admin`
- Password: `admin`

### 2. Verify Pre-configured Client

The Docker Compose setup automatically creates a test client:

- **Client ID:** `test-client`
- **Client Secret:** `test-client-secret-12345`
- **Realm:** `master`
- **Grant Type:** Client Credentials

These credentials are already configured in `appsettings.json`.

### 3. Start the API

From the API project directory:

```bash
cd src/Morsley.UK.Security.API
dotnet run
```

**API will be available at:** `https://localhost:7001`

**Swagger UI:** `https://localhost:7001/swagger`

### 4. Start the UI

From the UI project directory:

```bash
cd src/Morsley.UK.Security.UI
dotnet run
```

**UI will be available at:** `https://localhost:5001` (or the port shown in the console)

## Using the UI

The UI provides three buttons to demonstrate the OAuth 2.0 flow:

### 1. Check API Health
- Tests the `/health` endpoint (no authentication required)
- Verifies the API is running and accessible

### 2. Get Token
- Retrieves a JWT access token from Keycloak
- Uses the client credentials grant type
- Stores the token in memory for subsequent requests
- **Click this first before trying to access protected endpoints**

### 3. Get Greeting
- Calls the protected `/greeting` endpoint on the API
- Requires a valid JWT token (get one first using "Get Token")
- Demonstrates successful authentication with Bearer token

## OAuth 2.0 Flow

```
┌─────────┐                 ┌──────────┐                 ┌─────┐
│   UI    │                 │ Keycloak │                 │ API │
└────┬────┘                 └────┬─────┘                 └──┬──┘
     │                           │                          │
     │  1. Get Token             │                          │
     ├──────────────────────────>│                          │
     │  (client_id + secret)     │                          │
     │                           │                          │
     │  2. JWT Access Token      │                          │
     │<──────────────────────────┤                          │
     │                           │                          │
     │  3. API Request           │                          │
     │  (Authorization: Bearer token)                       │
     ├─────────────────────────────────────────────────────>│
     │                           │                          │
     │                           │  4. Validate Token       │
     │                           │<─────────────────────────┤
     │                           │  (verify signature, etc) │
     │                           │                          │
     │                           │  5. Token Valid          │
     │                           │──────────────────────────>│
     │                           │                          │
     │  6. Protected Resource    │                          │
     │<─────────────────────────────────────────────────────┤
     │                           │                          │
```

## Docker Compose Commands

### Start Keycloak
```bash
docker-compose up -d
```

### Stop Keycloak
```bash
docker-compose down
```

### View Logs
```bash
docker-compose logs -f keycloak
```

### Restart Keycloak
```bash
docker-compose restart keycloak
```

### Stop and Remove Everything (including volumes)
```bash
docker-compose down -v
```

**Note:** Use `-v` flag to remove volumes if you want to start completely fresh.

## Troubleshooting

### Keycloak Won't Start

**Check if port 8080 is already in use:**
```bash
# Windows
netstat -ano | findstr :8080

# Kill the process if needed
taskkill /PID <process_id> /F
```

### "Realm 'master' already exists. Import skipped"

This means Keycloak has existing data. To start fresh:
```bash
docker-compose down -v
docker-compose up -d
```

### Token Request Fails

1. Verify Keycloak is running: `http://localhost:8080/health`
2. Check the client exists in Keycloak admin console
3. Verify `appsettings.json` has the correct client secret
4. Check Docker logs: `docker-compose logs keycloak`

### API Returns 401 Unauthorized

1. Click "Get Token" first to obtain a valid JWT
2. Verify the token is stored (success message should appear)
3. Then click "Get Greeting"
4. If still failing, the token may have expired (get a new one)

### CORS Errors

The API has CORS enabled for all origins in development. If you see CORS errors:
1. Verify the API is running
2. Check the API URL in `appsettings.json` matches the actual API port
3. Restart the API

## Configuration

### appsettings.json

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7001"
  },
  "Keycloak": {
    "Url": "http://localhost:8080",
    "Realm": "master",
    "ClientId": "test-client",
    "ClientSecret": "test-client-secret-12345"
  }
}
```

Update these values if your ports or configuration differ.

## Learn More

- [OAuth 2.0 Specification](https://oauth.net/2/)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [JWT.io](https://jwt.io/) - Decode and inspect JWT tokens
