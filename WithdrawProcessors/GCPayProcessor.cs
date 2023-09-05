using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Tgs.Payment.GCPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class GCPayProcessor : IWithdrawProcessor
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

	private readonly JsonTransform _jsonTransform;

	public virtual string Subject => "tgs.payment.gcpay.withdraw";

	public GCPayProcessor(JsonTransform jsonTransform)
	{
		_jsonTransform = jsonTransform ?? throw new ArgumentNullException(nameof(jsonTransform));
	}

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var bankCode = brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString();

		var bankType = bankCode switch
		{
			"GCASH" => "GCash",
			"PAYMP" => "PayMaya",
			_ => "Bank"
		};

		var transformation = _bankInfoTransformations[bankType];
		var transforJson = _jsonTransform.Transform(bankInfo, transformation);

		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<GCPayBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var settings = new GCPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("customerId").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("key").GetString(),
			formId,
			brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString(),
			withdrawBankInfo.Detail.BankCardName,
			withdrawBankInfo.Detail.BankCardAccount);

		return JsonSerializer.Serialize(settings, GCPayUtility.SerializerOptions);
	}
}
