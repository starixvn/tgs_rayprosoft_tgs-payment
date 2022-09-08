using Tgs.Payment.Clients;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPaymentServiceClient(
		this IServiceCollection services,
		Action<IHttpClientBuilder> configureHttpClient)
	{
		configureHttpClient(services.AddHttpClient<PaymentServiceClient>());

		return services
			.AddTransient<IPaymentServiceClient>(sp => sp.GetRequiredService<PaymentServiceClient>());
	}
}
