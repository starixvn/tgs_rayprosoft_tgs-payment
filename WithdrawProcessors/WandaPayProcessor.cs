using System;
using System.Text.Json;
using Tgs.Payment.WandaPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class WandaPayProcessor : IWithdrawProcessor
{
	public string Subject => "tgs.payment.wandapay.withdraw";

	public string MakeWithdrawSettings(
		Guid formId,
		decimal amount,
		string bankInfo,
		BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<WandaPayBankInfo>>(bankInfo, PaymentUtility.SerializerOptions);
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var settings = new WandaPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("storeCode").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("signKey").GetString(),
			formId,
			withdrawBankInfo.Detail.AccountName,
			withdrawBankInfo.Detail.AccountNumber,
			brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString());

		return JsonSerializer.Serialize(settings, WandaPayUtility.SerializerOptions);
	}
}
