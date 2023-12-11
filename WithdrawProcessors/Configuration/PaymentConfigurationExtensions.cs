using Microsoft.Extensions.DependencyInjection;
using Tgs.Payment.AlfaPay;
using Tgs.Payment.Help2Pay;
using Tgs.Payment.LeePay;
using Tgs.Payment.MalakiPay;
using Tgs.Payment.MaticPay;
using Tgs.Payment.PrimePay;
using Tgs.Payment.SkyPay;
using Tgs.Payment.TuPay;
using Tgs.Payment.WithdrawProcessors;
using Tgs.Payment.ZeroNineNinePay;
using TGS.Payment.ManualDeposit;

namespace Tgs.Payment.Configuration;

public static class PaymentConfigurationExtensions
{
	public static PaymentConfiguration RegisterWithdrawProcessors(this PaymentConfiguration configuration)
	{
		_ = configuration.Services
			.AddSingleton<LeePaySerializer>()
			.AddSingleton<PrimePaySerializer>()
			.AddSingleton<ManualDepositSerializer>()
			.AddSingleton<MaticPaySerializer>()
			.AddSingleton<Help2PaySerializer>()
			.AddSingleton<SkyPaySerializer>()
			.AddSingleton<ZeroNineNinePaySerializer>()
			.AddSingleton<TuPaySerializer>()
			.AddSingleton<MalakiPaySerializer>()
			.AddSingleton<AlfaPaySerializer>();

		return configuration
			.RegisterWithdrawProcessor<LeePayProcessor>("LeePay")
			.RegisterWithdrawProcessor<PrimePayProcessor>("PrimePay")
			.RegisterWithdrawProcessor<ManualPayProcessor>("Manual")
			.RegisterWithdrawProcessor<MaticPayProcessor>("MaticPay")
			.RegisterWithdrawProcessor<Help2PayProcessor>("Help2Pay")
			.RegisterWithdrawProcessor<SkyPayVNProcessor>("SkyPay-VN")
			.RegisterWithdrawProcessor<SkyPayProcessor>("SkyPay-CN")
			.RegisterWithdrawProcessor<ZeroNineNinePayProcessor>("ZeroNineNinePay")
			.RegisterWithdrawProcessor<TuPayProcessor>("TuPay")
			.RegisterWithdrawProcessor<MalakiPayProcessor>("MalakiPay")
			.RegisterWithdrawProcessor<AlfaPayProcessor>("AlfaPay")
			.RegisterWithdrawProcessor<WandaPayProcessor>("WandaPay")
			.RegisterWithdrawProcessor<PhpGoPayProcessor>("PhpGoPay")
			.RegisterWithdrawProcessor<FiveSevenPayProcessor>("FiveSevenPay")
			.RegisterWithdrawProcessor<FiveSevenPayV2Processor>("FiveSevenPay-V2")
			.RegisterWithdrawProcessor<MetaPayProcessor>("MetaPay")
			.RegisterWithdrawProcessor<GCPayProcessor>("GCPay")
			.RegisterWithdrawProcessor<WinPayProcessor>("WinPay")
			.RegisterWithdrawProcessor<MalakiPayV2Processor>("MalakiPay-V2")
			.RegisterWithdrawProcessor<SBPayProcessor>("SBPay")
			.RegisterWithdrawProcessor<GanyoPayProcessor>("GanyoPay")
			.RegisterWithdrawProcessor<DibaoPayProcessor>("DibaoPay")
			.RegisterWithdrawProcessor<U2PayProcessor>("U2Pay");
	}
}
