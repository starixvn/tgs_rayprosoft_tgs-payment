using System.Reflection;
using JSLTSharp;
using JSLTSharp.JsonTransforms.Abstractions;
using JSLTSharp.JsonTransforms.Extensions;
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
			.AddTransient<IWithdrawService, WithdrawService>()
			.AddSingleton<IWithdrawProcessorFactory>(
				sp => ActivatorUtilities.CreateInstance<WithdrawProcessorFactory>(
					sp,
					configuration.GetAllRegisterWithdrawProcessorTypes()))
			.AddSingleton<IBankInfoIndexParserFactory, BankInfoIndexParserFactory>()
			.AddSingleton<ISystemClock, DefaultSystemClock>()
			.AddTransient<ICreateWithdrawFormPipe, EmitCreateWithdrawFormEventPipe>()
			.AddTransient<ICreateWithdrawFormPipe, PlayerCashOperationCreateWithdrawFormEventPipe>()
			.AddTransient<ICreateWithdrawFormPipe, CheckPlayerBucketCreateWithdrawEventPipe>()
			.AddSerialNumberGenerator()
			.AddOptions<BankInfoParserOptions>();

		services
			.AddSingleton(sp => new JsonTransform(
				sp.GetServices<IJsonTransformConditionalCustomOperation>(),
				sp.GetServices<IJsonTransformCustomOperation>()))
			.RegisterJsonCustomTransformFunctions();

		return configuration;
	}
}
