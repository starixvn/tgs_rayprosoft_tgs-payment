using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Tgs.Payment.ShunfaPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class ShunfaPayProcessor(JsonTransform jsonTransform) : IWithdrawProcessor
{
	private readonly IReadOnlyDictionary<string, string> _bankInfoTransformations = new Dictionary<string, string>()
	{
		["Bank"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"receiverName": "$.detail.name",
					"receiverAccount": "$.detail.bankAccount"
				}
			}
			""",
		["GCash"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"receiverName": "$.detail.phone",
					"receiverAccount": "$.detail.phone"
				}
			}
			"""
	}.AsReadOnly();

	public string Subject => "tgs.payment.shunfapay.withdraw";

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var bankCode = brandPaymentExtendedData.RootElement.GetProperty("bank").GetString();

		var bankType = bankCode switch
		{
			"gcash" => "GCash",			
			_ => "Bank"
		};

		var transformation = _bankInfoTransformations[bankType];

		var transforJson = jsonTransform.Transform(bankInfo, transformation);

		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<ShunfaPayBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var settings = new ShunfaPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("clientId").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("apiKey").GetString(),
			formId,
			bankCode,
			withdrawBankInfo.Detail.ReceiverName,
			withdrawBankInfo.Detail.ReceiverAccount);

		return JsonSerializer.Serialize(settings, ShunfaPayUtility.SerializerOptions);
	}
}