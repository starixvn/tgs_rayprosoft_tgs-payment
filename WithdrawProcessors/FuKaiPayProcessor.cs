using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Tgs.Payment.FuKaiPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class FuKaiPayProcessor(JsonTransform jsonTransform) : IWithdrawProcessor
{
	private readonly IReadOnlyDictionary<string, string> _bankInfoTransformations = new Dictionary<string, string>()
	{
		["Bank"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"payerName": "$.detail.name",
					"payerAccount": "$.detail.bankAccount"
				}
			}
			""",
		["GCASH"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"payerAccount": "$.detail.phone",
					"payerName": "$.detail.phone"
				}
			}
			""",
		["MAYA"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"payerAccount": "$.detail.phone",
					"payerName": "$.detail.phone"
				}
			}
			"""
	}.AsReadOnly();

	public string Subject => "tgs.payment.fukaipay.withdraw";

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var channelCode = brandPaymentExtendedData.RootElement.GetProperty("channelCode").GetString();

		var bankType = channelCode switch
		{
			"GCASH" => "GCASH",
			"MAYA" => "MAYA",
			_ => "Bank"
		};

		var transformation = _bankInfoTransformations[bankType];

		var transforJson = jsonTransform.Transform(bankInfo, transformation);

		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<FuKaiPayBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var settings = new FuKaiPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchantId").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("merchantKey").GetString(),
			formId,
			channelCode,
			withdrawBankInfo.Detail.PayerName,
			withdrawBankInfo.Detail.PayerAccount,
			brandPaymentExtendedData.RootElement.GetProperty("payerBankName").GetString(),
			"PHP",
			"127.0.0.1");

		return JsonSerializer.Serialize(settings, FuKaiPayUtility.SerializerOptions);
	}
}