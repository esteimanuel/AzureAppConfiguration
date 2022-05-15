using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureAppConfiguration(options =>
{
    options
        .Connect(builder.Configuration.GetConnectionString("AppConfig"))
        .ConfigureRefresh(refresh =>
        {
            refresh
                .Register("Web.App:Settings:Sentinel", refreshAll: true)
                .SetCacheExpiration(TimeSpan.FromDays(1));
        })
        .UseFeatureFlags();
});

builder.Services.Configure<Web.App.Settings>(builder.Configuration.GetSection("Web.App:Settings"));
builder.Services.AddAzureAppConfiguration();
builder.Services.AddFeatureManagement();

#region Hide
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

#region Hide
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html"); ;

app.Run();
#endregion
