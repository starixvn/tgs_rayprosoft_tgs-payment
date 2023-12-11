using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Tgs.Payment.U2Pay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class U2PayProcessor : IWithdrawProcessor
{
	private readonly IReadOnlyDictionary<string, string> _bankInfoTransformations = new Dictionary<string, string>()
	{
		["bank"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"name": "$.detail.name",
					"bankAccount": "$.detail.bankAccount"
				}
			}
			"""
	}.AsReadOnly();

	private readonly JsonTransform _jsonTransform;
	public string Subject => "tgs.payment.u2pay.withdraw";

	public U2PayProcessor(JsonTransform jsonTransform)
	{
		_jsonTransform = jsonTransform ?? throw new ArgumentNullException(nameof(jsonTransform));
	}

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var transformation = _bankInfoTransformations["bank"];
		var transforJson = _jsonTransform.Transform(bankInfo, transformation);
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<U2PayBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var settings = new U2PayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchantNo").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("appSecret").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("privateKey").GetString(),
			formId,
			ReceiverName: withdrawBankInfo.Detail.Name,
			brandPaymentExtendedData.RootElement.GetProperty("bankName").GetString(),
			withdrawBankInfo.Detail.BankAccount);

		return JsonSerializer.Serialize(settings, U2PayUtility.SerializerOptions);
	}
}
