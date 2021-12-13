using Microsoft.Extensions.DependencyInjection;
using Tgs.Payment.ApplePay;
using Tgs.Payment.Help2Pay;
using Tgs.Payment.LeePay;
using Tgs.Payment.MaticPay;
using Tgs.Payment.PrimePay;
using Tgs.Payment.SkyPay;
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
				.AddSingleton<MaticPaySerializer>()
				.AddSingleton<Help2PaySerializer>()
				.AddSingleton<SkyPaySerializer>();

			return configuration
				.RegisterWithdrawProcessor<LeePayProcessor>("LeePay")
				.RegisterWithdrawProcessor<PrimePayProcessor>("PrimePay")
				.RegisterWithdrawProcessor<ApplePayProcessor>("ApplePay")
				.RegisterWithdrawProcessor<ManualPayProcessor>("Manual")
				.RegisterWithdrawProcessor<MaticPayProcessor>("MaticPay")
				.RegisterWithdrawProcessor<Help2PayProcessor>("Help2Pay")
				.RegisterWithdrawProcessor<SkyPayProcessor>("SkyPay-VN", "vn")
				.RegisterWithdrawProcessor<SkyPayProcessor>("SkyPay-CN", "cn");
		}
	}
}
