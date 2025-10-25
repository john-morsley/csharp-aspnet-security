using System.Text.Json;

namespace Morsley.UK.Security.UI.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> CheckApiHealth()
    {
        try
        {
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            var httpClient = _httpClientFactory.CreateClient();
            
            var response = await httpClient.GetAsync($"{apiBaseUrl}/health");
            var content = await response.Content.ReadAsStringAsync();

            return Json(new
            {
                success = response.IsSuccessStatusCode,
                statusCode = (int)response.StatusCode,
                data = content
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling API health endpoint");
            return Json(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> GetGreeting([FromBody] TokenRequest request)
    {
        try
        {
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            var httpClient = _httpClientFactory.CreateClient();
            
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{apiBaseUrl}/greeting");
            
            if (!string.IsNullOrEmpty(request?.Token))
            {
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", request.Token);
            }
            
            var response = await httpClient.SendAsync(httpRequest);
            var content = await response.Content.ReadAsStringAsync();

            return Json(new
            {
                success = response.IsSuccessStatusCode,
                statusCode = (int)response.StatusCode,
                data = content
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling API greeting endpoint");
            return Json(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    public class TokenRequest
    {
        public string? Token { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> GetToken()
    {
        try
        {
            var keycloakUrl = _configuration["Keycloak:Url"];
            var realm = _configuration["Keycloak:Realm"];
            var clientId = _configuration["Keycloak:ClientId"];
            var clientSecret = _configuration["Keycloak:ClientSecret"];

            var httpClient = _httpClientFactory.CreateClient();
            
            var tokenEndpoint = $"{keycloakUrl}/realms/{realm}/protocol/openid-connect/token";
            
            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            });

            var response = await httpClient.PostAsync(tokenEndpoint, requestBody);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(content);
                var accessToken = tokenResponse.GetProperty("access_token").GetString();
                
                return Json(new
                {
                    success = true,
                    statusCode = (int)response.StatusCode,
                    token = accessToken
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    statusCode = (int)response.StatusCode,
                    error = content
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token from Keycloak");
            return Json(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
