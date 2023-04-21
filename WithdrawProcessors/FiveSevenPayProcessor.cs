#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Microsoft.Extensions.Options;
using Tgs.Payment.FiveSevenPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class FiveSevenPayProcessor : IWithdrawProcessor
{
	private readonly IReadOnlyDictionary<string, string> _bankInfoTransformations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["Gcash"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"bankCardName": "$.detail.phone",
					"bankCardAccount": "$.detail.phone"
				}
			}
			"""
	}.AsReadOnly();

	private readonly JsonTransform _jsonTransform;
	private readonly JsonSerializerOptions _paymentSerializerOptions;

	public string Subject => "tgs.payment.fivesevenpay.withdraw";

	public FiveSevenPayProcessor(
		IOptions<PaymentOptions> jsonOptionsAccessor,
		JsonTransform jsonTransform)
	{
		_jsonTransform = jsonTransform ?? throw new ArgumentNullException(nameof(jsonTransform));
		_paymentSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptionsAccessor));
	}

	public string MakeWithdrawSettings(
		Guid formId,
		decimal amount,
		string? bankInfo,
		BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, _paymentSerializerOptions)!;

		var payType = FiveSevenPayType.Parse(brandPaymentExtendedData.RootElement.GetProperty("payType").GetString()!)!;

		var transformation = _bankInfoTransformations[payType.Code];
		var transformJson = _jsonTransform.Transform(bankInfo, transformation);

		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<FiveSevenPayBankInfo>>(transformJson, _paymentSerializerOptions)!;

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
