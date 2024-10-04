using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Tgs.Payment.ApolloPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class ApolloPayProcessor(
	JsonTransform jsonTransform)
	: IWithdrawProcessor
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
					"bankCardNo": "$.detail.bankAccount"
				}
			}
			"""
	}.AsReadOnly();

	public string Subject => "tgs.payment.apollopay.withdraw";

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var withdrawInfo = JsonSerializer.Deserialize<WithdrawBankInfo<JsonDocument>>(bankInfo, PaymentUtility.SerializerOptions);

		var transformation = _bankInfoTransformations[withdrawInfo.BankType];

		var transforJson = jsonTransform.Transform(bankInfo, transformation);

		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<ApolloPayBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var settings = new ApolloPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchantNo").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("apiKey").GetString(),
			formId,
			brandPaymentExtendedData.RootElement.GetProperty("channelCode").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString(),
			withdrawBankInfo.Detail.BankCardNo,
			withdrawBankInfo.Detail.AccountName);

		return JsonSerializer.Serialize(settings, ApolloPayUtility.SerializerOptions);
	}
}