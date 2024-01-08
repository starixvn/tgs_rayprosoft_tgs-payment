using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Tgs.Payment.DibaoPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class DibaoPayProcessor(JsonTransform jsonTransform) : IWithdrawProcessor
{
	private readonly IReadOnlyDictionary<string, string> _bankInfoTransformations = new Dictionary<string, string>()
	{
		["bank"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"bankAccount": "$.detail.name",
					"bankNo": "$.detail.bankAccount"
				}
			}
			"""
	}.AsReadOnly();

	public string Subject => "tgs.payment.dibaopay.withdraw";

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var transformation = _bankInfoTransformations["bank"];
		var transforJson = jsonTransform.Transform(bankInfo, transformation);
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<DibaoPayBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var settings = new DibaoPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchantId").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("privateKey").GetString(),
			formId,
			"127.0.0.1",
			brandPaymentExtendedData.RootElement.GetProperty("bankId").GetString(),
			withdrawBankInfo.Detail.BankAccount,
			withdrawBankInfo.Detail.BankNo,
			brandPaymentExtendedData.RootElement.GetProperty("exchangeRate").GetDecimal());

		return JsonSerializer.Serialize(settings, DibaoPayUtility.SerializerOptions);
	}
}
