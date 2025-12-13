using Microsoft.Extensions.Logging;
using CRReservation.COMMON;
using CRReservation.COMMON.DependencyInjection;
using CRReservation.COMMON.Services;

namespace CRReservation.MOBILE
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				});
			
			// Configure services from COMMON
			DependencyInjection.InicializeDependencyInjection(builder.Services);
			
			// Configure MAUI-specific services
			builder.Services.AddMauiBlazorWebView();
			builder.Services.AddScoped<ITokenService, TokenService>();
			builder.Services.AddScoped(sp => new AuthorizingHttpClientHandler(sp.GetRequiredService<ITokenService>()));
			builder.Services.AddScoped(sp =>
				new HttpClient(sp.GetRequiredService<AuthorizingHttpClientHandler>())
				{
					BaseAddress = new Uri("http://localhost:5087") // Configure for your API
				});

#if DEBUG
			builder.Services.AddBlazorWebViewDeveloperTools();
			builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
	}
}

