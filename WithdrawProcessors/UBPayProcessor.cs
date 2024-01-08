using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Tgs.Payment.UBPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class UBPayProcessor(
	JsonTransform jsonTransform) : IWithdrawProcessor
{
	private readonly IReadOnlyDictionary<string, string> _bankInfoTransformations = new Dictionary<string, string>()
	{
		["Crypto"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"address": "$.detail.address"
				}
			}
			""",
	}.AsReadOnly();

	public string Subject => "tgs.payment.ubpay.withdraw";

	public string MakeWithdrawSettings(
		Guid formId,
		decimal amount,
		string bankInfo,
		BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var transformation = _bankInfoTransformations["Crypto"];
		var transforJson = jsonTransform.Transform(bankInfo, transformation);
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<UBPayBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var canGetCurrency = brandPaymentExtendedData.RootElement.TryGetProperty("currency", out var jsonCurrency);

		var settings = new UBPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("apiToken").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("notifyToken").GetString(),
			formId,
			withdrawBankInfo.Detail.Address,
			canGetCurrency ? jsonCurrency.GetString() : null,
			brandPaymentExtendedData.RootElement.GetProperty("exchangeRate").GetDecimal());

		return JsonSerializer.Serialize(settings, UBPayUtility.SerializerOptions);
	}
}
