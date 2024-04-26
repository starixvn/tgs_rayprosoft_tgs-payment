using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Tgs.Payment.HaloPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class HaloPayProcessor(JsonTransform jsonTransform) : IWithdrawProcessor
{
	private readonly IReadOnlyDictionary<string, string> _bankInfoTransformations = new Dictionary<string, string>()
	{
		["Bank"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"accountName": "$.detail.name",
					"accountNumber": "$.detail.bankAccount"
				}
			}
			""",
		["DANA"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"accountNumber": "$.detail.phone",
					"accountName": "$.detail.phone"
				}
			}
			"""
	}.AsReadOnly();

	public string Subject => "tgs.payment.halopay.withdraw";

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var bankId = brandPaymentExtendedData.RootElement.GetProperty("bankId").GetString();

		var bankType = bankId switch
		{
			"152" => "DANA",
			_ => "Bank"
		};

		var transformation = _bankInfoTransformations[bankType];

		var transforJson = jsonTransform.Transform(bankInfo, transformation);

		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<HaloPayBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var settings = new HaloPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchantId").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("apiKey").GetString(),
			formId,
			withdrawBankInfo.Detail.AccountNumber,
			withdrawBankInfo.Detail.AccountName,
			brandPaymentExtendedData.RootElement.GetProperty("bankId").GetString());

		return JsonSerializer.Serialize(settings, HaloPayUtility.SerializerOptions);
	}
}