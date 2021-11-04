using System;
using Microsoft.EntityFrameworkCore;
using Tgs.Payment.Configuration;
using Tgs.Payment.Repositories;
using Tgs.Payment.Repositories.Entities;
using TGS.Payment.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class PaymentConfigurationExtensions
	{
		public static PaymentConfiguration UseRedis(
			this PaymentConfiguration paymentConfiguration,
			Action<RedisOptions, IServiceProvider> dbConfigure)
		{
			paymentConfiguration.Services
				.AddOptions<RedisOptions>()
				.Configure(dbConfigure)
				.Services
				.AddSingleton<RedisConnectionFactory>();

			return paymentConfiguration
				.UseRepository<RedisTakeNumberStore>();
		}

		public static PaymentConfiguration UseTgsPaymentRepository(
			this PaymentConfiguration paymentConfiguration,
			Action<IServiceProvider, DbContextOptionsBuilder> dbConfigure)
		{
			paymentConfiguration
				.UsePaymentRepository<PaymentRepository>()
				.Services
				.AddDbContext<TgsContext>(dbConfigure);

			return paymentConfiguration;
		}
	}
}
