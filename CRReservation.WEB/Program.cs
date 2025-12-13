using CRReservation.COMMON;
using CRReservation.COMMON.DependencyInjection;
using CRReservation.COMMON.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

DependencyInjection.InicializeDependencyInjection(builder.Services);

// Configure HttpClient with authorization handler
var apiBaseAddress = builder.Configuration["API:BaseAddress"] ?? "http://localhost:5087";

// Register token and authentication services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<PersistentAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<PersistentAuthenticationStateProvider>());

// Register the authorization handler
builder.Services.AddScoped(sp => new AuthorizingHttpClientHandler(sp.GetRequiredService<ITokenService>()));
builder.Services.AddScoped(sp =>
    new HttpClient(sp.GetRequiredService<AuthorizingHttpClientHandler>())
    {
        BaseAddress = new Uri(apiBaseAddress)
    });

// Add authorization support
builder.Services.AddAuthorizationCore();

var host = builder.Build();

// Initialize TokenService and restore authentication state
await InitializeAppAsync(host.Services);

await host.RunAsync();

static async Task InitializeAppAsync(IServiceProvider services)
{
    var tokenService = services.GetRequiredService<ITokenService>();
    var authProvider = services.GetRequiredService<PersistentAuthenticationStateProvider>();
    
    try
    {
        await tokenService.InitializeAsync();
        await authProvider.GetAuthenticationStateAsync();
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error during app initialization: {ex.Message}");
    }
}


