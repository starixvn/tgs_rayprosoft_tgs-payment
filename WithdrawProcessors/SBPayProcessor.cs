using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Tgs.Payment.SBPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class SBPayProcessor : IWithdrawProcessor
{
	private readonly IReadOnlyDictionary<string, string> _bankInfoTransformations = new Dictionary<string, string>()
	{
		["Bank"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"bankOwner": "$.detail.name",
					"accountNumber": "$.detail.bankAccount"
				}
			}
			""",
	}.AsReadOnly();

	private readonly JsonTransform _jsonTransform;
	public string Subject => "tgs.payment.sbpay.withdraw";

	public SBPayProcessor(JsonTransform jsonTransform)
	{
		_jsonTransform = jsonTransform ?? throw new ArgumentNullException(nameof(jsonTransform));
	}

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var transformation = _bankInfoTransformations["Bank"];
		var transforJson = _jsonTransform.Transform(bankInfo, transformation);
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<SBPayBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var settings = new SBPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("apiToken").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("notifyToken").GetString(),
			formId,
			brandPaymentExtendedData.RootElement.GetProperty("bankId").GetString(),
			withdrawBankInfo.Detail.BankOwner,
			withdrawBankInfo.Detail.AccountNumber,
			brandPaymentExtendedData.RootElement.GetProperty("exchangeRate").GetDecimal());

		return JsonSerializer.Serialize(settings, SBPayUtility.SerializerOptions);
	}
}
