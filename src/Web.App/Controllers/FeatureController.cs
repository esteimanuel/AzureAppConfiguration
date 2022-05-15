using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace Web.App.Controllers;

[ApiController]
[Route("api/[controller]")]
// [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class FeaturesController : ControllerBase
{
    private readonly ILogger<FeaturesController> _logger;
    private readonly IFeatureManagerSnapshot featureManager;

    public FeaturesController(ILogger<FeaturesController> logger, IFeatureManagerSnapshot featureManager)
    {
        _logger = logger;
        this.featureManager = featureManager;
    }

    [HttpGet]
    public async IAsyncEnumerable<string> Get()
    {
        await foreach(var featureName in this.featureManager.GetFeatureNamesAsync()) 
        {
            yield return featureName;
        }
    }
}
