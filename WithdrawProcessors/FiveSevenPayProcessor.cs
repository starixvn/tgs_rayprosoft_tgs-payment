#nullable enable

using System;
using System.Text.Json;
using Tgs.Payment.FiveSevenPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class FiveSevenPayProcessor : IWithdrawProcessor
{
	public string Subject => "tgs.payment.fivesevenpay.withdraw";

	public string MakeWithdrawSettings(
		Guid formId,
		decimal amount,
		string? bankInfo,
		BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<FiveSevenPayBankInfo>>(bankInfo!, PaymentUtility.SerializerOptions)!;

		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions)!;

		var payType = FiveSevenPayType.Parse(brandPaymentExtendedData.RootElement.GetProperty("payType").GetString()!)!;

		var settings = new FiveSevenPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchantKey").GetString()!,
			brandPaymentExtendedData.RootElement.GetProperty("privateKey").GetString()!,
			brandPaymentExtendedData.RootElement.GetProperty("publicKey").GetString()!,
			brandPaymentExtendedData.RootElement.GetProperty("currency").GetString()!,
			payType,
			formId,
			withdrawBankInfo.Detail!.BankCardName,
			withdrawBankInfo.Detail.BankCardAccount);

		return JsonSerializer.Serialize(settings, FiveSevenPayUtility.SerializerOptions);
	}
}
