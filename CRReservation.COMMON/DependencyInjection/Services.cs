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
			services.AddScoped<IClassRoomService, ClassRoomService>();
			// TokenService is registered in the host-specific Program.cs (WEB/MOBILE)
		}
	}
}

