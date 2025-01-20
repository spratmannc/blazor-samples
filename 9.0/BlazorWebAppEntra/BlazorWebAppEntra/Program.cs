using System.Security.Claims;
using BlazorWebAppEntra.Client.Weather;
using BlazorWebAppEntra.Components;
using BlazorWebAppEntra.Weather;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Groups.Item.MembersWithLicenseErrors;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services
       .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
       .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
       .EnableTokenAcquisitionToCallDownstreamApi()
       .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
       //.AddDownstreamApi("MicrosoftGraph", builder.Configuration.GetSection("MicrosoftGraph"))
       .AddInMemoryTokenCaches()
       ;

builder.Services.AddAuthorization();



builder.Services.AddScoped<IWeatherForecaster, ServerWeatherForecaster>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapGet("/weather-forecast", async (ClaimsPrincipal user, IAuthorizationHeaderProvider provider,[FromServices] IWeatherForecaster WeatherForecaster) =>
{
    var header = await provider.CreateAuthorizationHeaderForUserAsync(["user.read"]);

    return WeatherForecaster.GetWeatherForecastAsync();

}).RequireAuthorization();


app.MapGet("/my-photo", async (GraphServiceClient graph) =>
{
    var result = await graph.Me.Photo.Content.GetAsync();

    return Results.File(result!, contentType: "image/jpeg");

}).RequireAuthorization();


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebAppEntra.Client._Imports).Assembly);

app.MapGroup("/authentication").MapLoginAndLogout();

app.Run();
