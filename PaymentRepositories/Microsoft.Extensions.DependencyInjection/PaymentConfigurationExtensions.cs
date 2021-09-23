using System;
using Microsoft.EntityFrameworkCore;
using Tgs.Payment.Configuration;
using Tgs.Payment.Repositories.Entities;
using TGS.Payment.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class PaymentConfigurationExtensions
    {
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