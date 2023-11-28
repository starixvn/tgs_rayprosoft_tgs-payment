using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Tgs.Payment.GanyoPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class GanyoPayProcessor : IWithdrawProcessor
{
	private readonly IReadOnlyDictionary<string, string> _bankInfoTransformations = new Dictionary<string, string>()
	{
		["bkash"] = """
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
		["nagad"] = """
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
		["rocket"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"accountName": "$.detail.name",
					"accountNumber": "$.detail.bankAccount"
				}
			}
			"""
	}.AsReadOnly();

	private readonly JsonTransform _jsonTransform;
	public string Subject => "tgs.payment.ganyopay.withdraw";

	public GanyoPayProcessor(JsonTransform jsonTransform)
	{
		_jsonTransform = jsonTransform ?? throw new ArgumentNullException(nameof(jsonTransform));
	}

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var bankName = brandPaymentExtendedData.RootElement.GetProperty("bankName").GetString();

		var transformation = _bankInfoTransformations[bankName];
		var transforJson = _jsonTransform.Transform(bankInfo, transformation);
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<GanyoPayBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var settings = new GanyoPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("customerId").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("privateKey").GetString(),
			formId,
			withdrawBankInfo.Detail.AccountNumber,
			withdrawBankInfo.Detail.AccountName,
			bankName,
			null);

		return JsonSerializer.Serialize(settings, GanyoPayUtility.SerializerOptions);
	}
}
