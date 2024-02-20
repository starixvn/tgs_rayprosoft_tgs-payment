using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Tgs.Payment.AlfaPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class AlfaPayProcessor(JsonTransform jsonTransform) : IWithdrawProcessor
{
	private readonly IReadOnlyDictionary<string, string> _bankInfoTransformations = new Dictionary<string, string>()
	{
		["Bank"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"bankCardName": "$.detail.name",
					"bankCardAccount": "$.detail.bankAccount"
				}
			}
			""",
		["GCash"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"bankCardName": "$.detail.phone",
					"bankCardAccount": "$.detail.phone"
				}
			}
			""",
		["PayMaya"] = """
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

	public string Subject => "tgs.payment.alfapay.withdraw";

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var bankCode = brandPaymentExtendedData.RootElement.GetProperty("bankId").GetString();

		var bankType = bankCode switch
		{
			"GCASH" => "GCash",
			"PAYMAYA" => "PayMaya",
			_ => "Bank"
		};

		var transformation = _bankInfoTransformations[bankType];

		var transforJson = jsonTransform.Transform(bankInfo, transformation);

		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<AlfaPayBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var settings = new AlfaPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("apiToken").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("notifyToken").GetString(),
			withdrawBankInfo.Detail.AccountNumber,
			brandPaymentExtendedData.RootElement.GetProperty("bankId").GetString(),
			withdrawBankInfo.Detail.BankOwner,
			formId);

		return JsonSerializer.Serialize(settings, AlfaPayUtility.SerializerOptions);
	}
}
