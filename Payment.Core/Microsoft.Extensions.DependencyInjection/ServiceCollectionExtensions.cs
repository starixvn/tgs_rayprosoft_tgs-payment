using System.Reflection;
using Tgs.Payment;
using Tgs.Payment.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static PaymentConfiguration AddPayment(this IServiceCollection services)
		{
			var configuration = new PaymentConfiguration(services);

			services.RegisterAllServiceUtilityTypes(
				Assembly.GetExecutingAssembly(),
				ServiceLifetime.Transient);

			services.AddSingleton<IWithdrawProcessorFactory>(
				sp => ActivatorUtilities.CreateInstance<WithdrawProcessorFactory>(
					sp,
					configuration.GetAllRegisterWithdrawProcessorTypes()));

			return configuration;
		}
	}
}
