using Tgs.Payment.Clients;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPaymentClient(this IServiceCollection services)
		=> services
			.AddTransient<IPaymentClient, PaymentClient>();
}
