using Tgs.Payment;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddPaymentClient(this IServiceCollection services)
			=> services
				.AddTransient<IPaymentClient, PaymentClient>();
	}
}
