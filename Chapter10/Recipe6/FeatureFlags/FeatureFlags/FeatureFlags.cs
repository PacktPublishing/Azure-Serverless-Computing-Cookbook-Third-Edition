using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Options;

namespace FeatureFlags
{
    public class FeatureFlags
    {
        private readonly IFeatureManagerSnapshot _featureManagerSnapshot;
        private readonly Settings _settings;
        private readonly IConfiguration _configuration;
        public FeatureFlags(IFeatureManagerSnapshot featureManagerSnapshot, IOptionsSnapshot<Settings> settings ,IConfiguration configuration)
        {
            _featureManagerSnapshot = featureManagerSnapshot;
            _settings = settings.Value;
            _configuration = configuration;

        }         
        [FunctionName("FeatureFlagsDemo")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            bool featureEnalbed = await _featureManagerSnapshot.IsEnabledAsync("TurnOnGreeting");

            if (featureEnalbed)
            {
                return new OkObjectResult($"Hello, {name}. {_settings.Greeting}");
            }
            else
            {
                return new OkObjectResult($"Hello, {name}.");
                
            }
        }
    }
}
