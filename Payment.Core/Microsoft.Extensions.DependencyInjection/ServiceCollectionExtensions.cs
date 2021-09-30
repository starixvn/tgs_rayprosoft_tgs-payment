using System.Reflection;
using Microsoft.Extensions.Configuration;
using Tgs.Payment;
using Tgs.Payment.Configuration;
using Tgs.Payment.MessageQueue;

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

			services
				.AddSingleton<IWithdrawProcessorFactory>(
					sp => ActivatorUtilities.CreateInstance<WithdrawProcessorFactory>(
						sp,
						configuration.GetAllRegisterWithdrawProcessorTypes()))
				.AddOptions<PaymentOptions>()
				.Services
				.AddNatsMessageQueue(config =>
				{
					config
						.ConfigQueueOptions((options, sp) =>
						{
							var configuration = sp.GetRequiredService<IConfiguration>();

							options.Url = configuration.GetConnectionString("Nats");
						})
						.AddHandler<SaveWithdrawThirdNoProcessor>("tgs.payment.withdraw.external_id");
				});

			return configuration;
		}
	}
}