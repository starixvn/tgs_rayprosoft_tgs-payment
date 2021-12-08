using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Tgs.Payment.Configuration
{
	public class PaymentConfiguration
	{
		private Dictionary<string, Type> _processTypes = new Dictionary<string, Type>();

		public IServiceCollection Services { get; }

		public PaymentConfiguration(IServiceCollection services)
		{
			Services = services ?? throw new System.ArgumentNullException(nameof(services));
		}

		public PaymentConfiguration ConfigureOptions(Action<PaymentOptions, IServiceProvider> configure)
		{
			Services
				.AddOptions<PaymentOptions>()
				.Configure(configure);

			return this;
		}

		public PaymentConfiguration RegisterWithdrawProcessor<TProcessor>(string paymentCode, params object[] parameters) where TProcessor : class, IWithdrawProcessor
		{
			Services.AddSingleton(
				sp => ActivatorUtilities.CreateInstance<TProcessor>(sp, parameters));

			_processTypes[paymentCode] = typeof(TProcessor);

			return this;
		}

		public PaymentConfiguration UsePaymentRepository<TPaymentRepository>() where TPaymentRepository : class, IPaymentRepository
		{
			Services.AddTransient<IPaymentRepository, TPaymentRepository>();
			return this;
		}

		public PaymentConfiguration UseRepository<TRepository>() where TRepository : class, ITakeNumberStore
		{
			Services.AddTransient<ITakeNumberStore, TRepository>();
			return this;
		}

		internal IDictionary<string, Type> GetAllRegisterWithdrawProcessorTypes() => _processTypes;
	}
}
