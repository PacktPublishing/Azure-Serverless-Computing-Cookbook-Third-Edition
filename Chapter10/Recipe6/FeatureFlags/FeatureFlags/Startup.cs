using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

[assembly: FunctionsStartup(typeof(FeatureFlags.Startup))]

namespace FeatureFlags
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddAzureAppConfiguration(options =>
            {
                options.Connect(Environment.GetEnvironmentVariable("ConnectionString"))
                       .UseFeatureFlags();
            });
            IConfiguration configuration = configurationBuilder.Build();
            
            builder.Services.Configure<Settings>(configuration.GetSection("CookbookApp:Settings"));
            builder.Services.AddFeatureManagement(configuration);
        }
    }
}
