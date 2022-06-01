using Azure.Messaging.EventGrid;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using System;
using System.Threading.Tasks;

namespace AzureAppConfiguration.Console.DotnetFramework
{
    public class Program
    {
        private static IConfigurationRefresher configurationRefresher;

        public static async Task<int> Main(string[] args)
        {
            var builder = new ConfigurationBuilder();

            var configuration =
                builder
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddJsonFile("appsettings.Development.json")
                    .AddUserSecrets<Program>()
                    .Build();

            builder
                .AddAzureAppConfiguration(options =>
                {
                    options
                        .Connect(configuration.GetConnectionString("AppConfig"))
                        .ConfigureRefresh(refresh =>
                        {
                            refresh
                            .Register("Settings:Sentinel", refreshAll: true)
                            .SetCacheExpiration(TimeSpan.FromDays(1)); // Reduce poll frequency
                        })
                        .UseFeatureFlags();

                    configurationRefresher = options.GetRefresher();
                });

            configuration = builder.Build();

            var services = new ServiceCollection();

            await RegisterRefreshEventHandler(configuration, configurationRefresher);

            services.AddSingleton<IConfiguration>(configuration).AddFeatureManagement();

            await FeatureManagementBackgroundProcess(services);

            while (System.Console.ReadLine() != "q");

            return 0;
        }

        static async Task RegisterRefreshEventHandler(IConfiguration configuration, IConfigurationRefresher refresher)
        {
            string serviceBusConnectionString = configuration.GetConnectionString("ServiceBus");
            string serviceBusTopic = configuration.GetValue<string>("AzureAppConfig:ServiceBus:Topic");
            string serviceBusSubscription = configuration.GetValue<string>("AzureAppConfig:ServiceBus:TopicSubscription");

            var serviceBusClient = new ServiceBusClient(serviceBusConnectionString);

            var processor = serviceBusClient.CreateProcessor(serviceBusTopic, serviceBusSubscription);

            processor.ProcessMessageAsync += async (args) =>
            {
                var message = args.Message;
                
                var eventGridEvent = EventGridEvent.Parse(BinaryData.FromBytes(message.Body));
                eventGridEvent.TryCreatePushNotification(out PushNotification pushNotification);
                
                var maxDelay = TimeSpan.FromSeconds(10); ;
                refresher.ProcessPushNotification(pushNotification, maxDelay: maxDelay);
                
                System.Console.WriteLine($"Config updated, wait for max delay of {maxDelay.Seconds} seconds to refresh configuration");
                await Task.Delay(maxDelay);
                
                var refreshed = await refresher.TryRefreshAsync();
                if (refreshed) System.Console.WriteLine($"Config refreshed");
            };

            processor.ProcessErrorAsync += (exceptionargs) =>
            {
                System.Console.WriteLine($"{exceptionargs.Exception}");
                return Task.CompletedTask;
            };

            System.Console.WriteLine("Start listening for config changes");
            await processor.StartProcessingAsync();
        }

        private static async Task FeatureManagementBackgroundProcess(ServiceCollection services)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    using (var serviceProvider = services.BuildServiceProvider())
                    {
                        var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

                        var feature = "beta";
                        System.Console.Write($"Get feature {feature} from cache, ");
                        if (await featureManager.IsEnabledAsync("beta"))
                        {
                            System.Console.WriteLine($"feature {feature} is enabled");
                        }
                        else
                        {
                            System.Console.WriteLine($"feature {feature} is disabled");
                        }
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });
        }
    }
}