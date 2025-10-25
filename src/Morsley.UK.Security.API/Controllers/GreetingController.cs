namespace Morsley.UK.Security.API.Controllers;

[ApiController]
[Route("greeting")]
[Authorize]
public class GreetingController : ControllerBase
{
    private readonly ILogger<GreetingController> _logger;

    public GreetingController(ILogger<GreetingController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public string Get()
    {
        _logger.LogInformation("Get called");
        return "Hello";
    }
}