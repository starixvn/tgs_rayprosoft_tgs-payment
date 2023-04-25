#nullable enable

using System;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.FiveSevenPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class FiveSevenPayProcessor : IWithdrawProcessor
{
	private readonly JsonSerializerOptions _paymentSerializerOptions;

	public string Subject => "tgs.payment.fivesevenpay.withdraw";

	public FiveSevenPayProcessor(
		IOptions<PaymentOptions> jsonOptionsAccessor)
	{
		_paymentSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptionsAccessor));
	}

	public string MakeWithdrawSettings(
		Guid formId,
		decimal amount,
		string? bankInfo,
		BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<FiveSevenPayBankInfo>>(bankInfo!, _paymentSerializerOptions)!;

		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, _paymentSerializerOptions)!;

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
