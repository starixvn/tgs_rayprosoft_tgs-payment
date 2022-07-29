using System.Reflection;
using Tgs.Payment;
using Tgs.Payment.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static PaymentConfiguration AddPayment(this IServiceCollection services)
	{
		var configuration = new PaymentConfiguration(services);

		_ = services.RegisterAllServiceUtilityTypes(
			Assembly.GetExecutingAssembly(),
			ServiceLifetime.Transient);

		_ = services
			.AddSingleton<IWithdrawProcessorFactory>(
				sp => ActivatorUtilities.CreateInstance<WithdrawProcessorFactory>(
					sp,
					configuration.GetAllRegisterWithdrawProcessorTypes()))
			.AddOptions<PaymentOptions>();

		return configuration;
	}
}
