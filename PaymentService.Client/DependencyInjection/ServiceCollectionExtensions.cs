using Microsoft.Extensions.Options;
using Tgs.Payment.Clients;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPaymentServiceClient(
		this IServiceCollection services,
		Action<PaymentServiceClientOptions, IServiceProvider> configure)
		=> services
			.AddOptions<PaymentServiceClientOptions>()
			.Configure(configure)
			.Services
			.AddHttpClient<PaymentServiceClient, PaymentServiceClient>(
				(httpClient, sp) => ActivatorUtilities.CreateInstance<PaymentServiceClient>(
					sp,
					httpClient,
					sp.GetRequiredService<IOptions<PaymentServiceClientOptions>>().Value.JsonSerializerOptions!))
			.ConfigureHttpClient((sp, httpClient) =>
			{
				var options = sp.GetRequiredService<IOptions<PaymentServiceClientOptions>>().Value;

				httpClient.BaseAddress = options.PaymentServiceBaseUrl;
			})
			.Services
			.AddTransient<IPaymentServiceClient>(sp => sp.GetRequiredService<PaymentServiceClient>());
}
