using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace Web.App.Controllers;

[ApiController]
[Route("api/[controller]")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class FeaturesController : ControllerBase
{
    private readonly ILogger<FeaturesController> _logger;
    private readonly IFeatureManager featureManager;

    public FeaturesController(ILogger<FeaturesController> logger, IFeatureManager featureManager)
    {
        _logger = logger;
        this.featureManager = featureManager;
    }

    [HttpGet]
    public async IAsyncEnumerable<Feature> Get()
    {
        await foreach(var featureName in this.featureManager.GetFeatureNamesAsync()) 
        {
            yield return new Feature 
            { 
                Name = featureName, 
                IsEnabled = await this.featureManager.IsEnabledAsync(featureName) 
            };
        }
    }
}
