using Tgs.Payment.WithdrawProcessors;

namespace Tgs.Payment.Configuration
{
	public static class PaymentConfigurationExtensions
	{
		public static PaymentConfiguration RegisterWithdrawProcessors(this PaymentConfiguration configuration)
			=> configuration
				.RegisterWithdrawProcessor<LeePayProcessor>("LeePay")
				.RegisterWithdrawProcessor<PrimePayProcessor>("PrimePay")
				.RegisterWithdrawProcessor<ApplePayProcessor>("ApplePay");
	}
}
