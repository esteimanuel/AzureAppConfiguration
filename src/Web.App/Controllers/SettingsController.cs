using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Web.App.Controllers;

[ApiController]
[Route("api/[controller]")]
// [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class SettingsController : ControllerBase
{
    private readonly ILogger<SettingsController> _logger;
    private readonly IOptionsSnapshot<Settings> settings;

    public SettingsController(ILogger<SettingsController> logger, IOptionsSnapshot<Settings> settings)
    {
        _logger = logger;
        this.settings = settings;
    }

    [HttpGet]
    public Settings Get()
    {
        return this.settings.Value;
    }
}
