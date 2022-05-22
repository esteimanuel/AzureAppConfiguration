using Azure.Messaging.EventGrid;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration.Extensions;
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

#region Dynamic configuration using poll model
//builder.Configuration
//.AddAzureAppConfiguration(options =>
//{
//    options
//    .Connect(builder.Configuration.GetConnectionString("AppConfig"))
//    .ConfigureRefresh(refresh =>
//    {
//        refresh
//        .Register("Web.App:Settings:Sentinel", refreshAll: true)
//        .SetCacheExpiration(TimeSpan.FromMinutes(5));
//    })
//    .UseFeatureFlags();
//});
#endregion

#region Dynamic configuration using push model
var refresher = null as IConfigurationRefresher;
builder.Configuration
.AddAzureAppConfiguration(options =>
{
    options
    .Connect(builder.Configuration.GetConnectionString("AppConfig"))
    .ConfigureRefresh(refresh =>
    {
        refresh
        .Register("Web.App:Settings:Sentinel", refreshAll: true)
        .SetCacheExpiration(TimeSpan.FromDays(1)); // Reduce poll frequency
    })
    .UseFeatureFlags();
    
    refresher = options.GetRefresher();

});

RegisterRefreshEventHandler(builder.Configuration, refresher);

static void RegisterRefreshEventHandler(ConfigurationManager configuration, IConfigurationRefresher refresher) {
    string serviceBusConnectionString = configuration.GetConnectionString("ServiceBus");
    string serviceBusTopic = "sbt-az-app-config-update-topic";
    string serviceBusSubscription = "sb-este-kn-app-config";
    var serviceBusClient = new SubscriptionClient(serviceBusConnectionString, serviceBusTopic, serviceBusSubscription);

    serviceBusClient.RegisterMessageHandler(
        handler: (message, cancellationToken) =>
        {
            // Build EventGridEvent from notification message
            var eventGridEvent = EventGridEvent.Parse(BinaryData.FromBytes(message.Body));

            // Create PushNotification from eventGridEvent
            eventGridEvent.TryCreatePushNotification(out PushNotification pushNotification);

            // Prompt Configuration Refresh based on the PushNotification
            refresher.ProcessPushNotification(pushNotification, maxDelay: TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        },
        exceptionReceivedHandler: (exceptionargs) =>
        {
            Console.WriteLine($"{exceptionargs.Exception}");
            return Task.CompletedTask;
        });
}
#endregion

builder.Services.Configure<Web.App.Settings>(builder.Configuration.GetSection("Web.App:Settings"));
builder.Services.AddAzureAppConfiguration();
builder.Services.AddFeatureManagement();

#region Off topic
// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
#endregion

app.UseAzureAppConfiguration();

#region Off topic
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html"); ;

app.Run();
#endregion
