using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Tgs.Payment.PrimePay;

namespace Tgs.Payment.WithdrawProcessors;

internal class PrimePayProcessor(
	PrimePaySerializer primePaySerializer,
	JsonTransform jsonTransform) : IWithdrawProcessor
{
	private readonly Dictionary<string, string> _bankInfoTransformations = new()
	{
		["Bank"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": "$.detail"
			}
			""",
		["FPS"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"bankName": "FPS",
					"bankAccount": "$.detail.phone",
					"name": "$.detail.username"
				}
			}
			"""
	};

	public string Subject => "tgs.payment.primepay.withdraw";

	public string MakeWithdrawSettings(
		Guid formId,
		decimal amount,
		string bankInfo,
		BrandPaymentInfo brandPayment)
	{
		var jsonDoc = JsonDocument.Parse(bankInfo);
		var bankType = jsonDoc.RootElement.GetProperty("bankType").GetString();

		var transformation = _bankInfoTransformations[bankType];
		var transforJson = jsonTransform.Transform(bankInfo, transformation);

		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<PrimePayBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var settings = new PrimePayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchantId").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("key").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("iv").GetString(),
			formId,
			brandPaymentExtendedData.RootElement.GetProperty("currency").GetString(),
			withdrawBankInfo.Detail.BankAccount,
			withdrawBankInfo.Detail.Name,
			withdrawBankInfo.Detail.BankBranch,
			withdrawBankInfo.Detail.BankName);

		return primePaySerializer.Serialize(settings);
	}
}
