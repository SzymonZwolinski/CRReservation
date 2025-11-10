using Microsoft.Extensions.DependencyInjection;

namespace CRReservation.COMMON.DependencyInjection
{
	public static class DependencyInjection
	{
		public static void InicializeDependencyInjection(IServiceCollection services)
		{
			Services.InicializeServices(services);
		}
	}
}
