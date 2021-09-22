using Microsoft.Extensions.DependencyInjection;
using Tgs.Payment.ApplePay;
using Tgs.Payment.LeePay;
using Tgs.Payment.MaticPay;
using Tgs.Payment.PrimePay;
using Tgs.Payment.WithdrawProcessors;
using TGS.Payment.ManualDeposit;

namespace Tgs.Payment.Configuration
{
	public static class PaymentConfigurationExtensions
    {
        public static PaymentConfiguration RegisterWithdrawProcessors(this PaymentConfiguration configuration)
        {
            configuration.Services
                .AddSingleton<LeePaySerializer>()
                .AddSingleton<PrimePaySerializer>()
                .AddSingleton<ApplePaySerializer>()
                .AddSingleton<ManualDepositSerializer>()
                .AddSingleton<MaticPaySerializer>();

            return configuration
                .RegisterWithdrawProcessor<LeePayProcessor>("LeePay")
                .RegisterWithdrawProcessor<PrimePayProcessor>("PrimePay")
                .RegisterWithdrawProcessor<ApplePayProcessor>("ApplePay")
                .RegisterWithdrawProcessor<ManualPayProcessor>("Manual")
                .RegisterWithdrawProcessor<MaticPayProcessor>("MaticPay");
        }
    }
}