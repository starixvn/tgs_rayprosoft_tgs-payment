using System;
using System.Text.Json;
using Tgs.Payment.ZeroNineNinePay;

namespace Tgs.Payment.WithdrawProcessors
{
	public class ZeroNineNinePayProcessor : IWithdrawProcessor
	{
		public string Subject => "tgs.payment.zeronineninepay.withdraw";

		public ZeroNineNinePayProcessor()
		{
		}

		public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
		{
			var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<ZeroNineNinePayBankInfo>>(bankInfo);

			var brandPaymentExtendedData = JsonSerializer
				.Deserialize<JsonDocument>(brandPayment.ExtendedData);

			var settings = new ZeroNineNinePayWithdrawSettings(
				brandPaymentExtendedData.RootElement.GetProperty("merchantId").GetInt32(),
				brandPaymentExtendedData.RootElement.GetProperty("merchantKey").GetString(),
				withdrawBankInfo.Detail.Name,
				brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString(),
				withdrawBankInfo.Detail.BankAccount,
				withdrawBankInfo.Detail.BankBranch,
				formId);

			return JsonSerializer.Serialize(settings);
		}
	}
}
