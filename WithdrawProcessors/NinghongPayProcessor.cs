using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Tgs.Payment.NinghongPay.Client;


namespace Tgs.Payment.WithdrawProcessors;

internal class NinghongPayProcessor(
	JsonTransform jsonTransform)
	: IWithdrawProcessor
{
	private readonly IReadOnlyDictionary<string, string> _bankInfoTransformations = new Dictionary<string, string>()
	{
		["ThirdParty"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"accountName": "$.detail.phone",
					"accountNo": "$.detail.phone"
				}
			}
			""",
		["Bank"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"accountName": "$.detail.name",
					"accountNo": "$.detail.bankAccount"
				}
			}
			"""
	}.AsReadOnly();

	public string Subject => "tgs.payment.ninghongpay.withdraw";

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var withdrawInfo = JsonSerializer.Deserialize<WithdrawBankInfo<JsonDocument>>(bankInfo, PaymentUtility.SerializerOptions);

		var transformation = _bankInfoTransformations[withdrawInfo.BankType];

		var transforJson = jsonTransform.Transform(bankInfo, transformation);

		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<NinghongPayBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var settings = new NinghongPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchantCode").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("secretKey").GetString(),
			formId,
			brandPaymentExtendedData.RootElement.GetProperty("method").GetInt32(),
			brandPaymentExtendedData.RootElement.GetProperty("bankName").GetString(),
			withdrawBankInfo.Detail.AccountNo,
			withdrawBankInfo.Detail.AccountName);

		return JsonSerializer.Serialize(settings, NinghongPayUtility.SerializerOptions);
	}
}