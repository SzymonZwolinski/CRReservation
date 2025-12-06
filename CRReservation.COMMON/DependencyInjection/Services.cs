using CRReservation.COMMON.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CRReservation.COMMON.DependencyInjection
{
	public static class Services
	{
		internal static void InicializeServices(IServiceCollection services)
		{
			services.AddScoped<UserStateService>();
			services.AddScoped<AuthService>();
			services.AddScoped(sp => new HttpClient
			{
				BaseAddress = new Uri("http://localhost:5000")
			});

		}
	}
}
