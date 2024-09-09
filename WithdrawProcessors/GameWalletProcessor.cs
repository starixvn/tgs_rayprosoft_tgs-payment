using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Payment.GameWallet.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class GameWalletProcessor(JsonTransform jsonTransform) : IWithdrawProcessor
{
	public string Subject => "tgs.payment.gamewallet.withdraw";

	private readonly IReadOnlyDictionary<string, string> _bankInfoTransformations = new Dictionary<string, string>()
	{
		["Bank"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"bankAccountName": "$.detail.name",
					"bankAccountNumber": "$.detail.bankAccount"
				}
			}
			"""
	}.AsReadOnly();

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var withdrawInfo = JsonSerializer.Deserialize<WithdrawBankInfo<JsonDocument>>(bankInfo, PaymentUtility.SerializerOptions);

		var transformation = _bankInfoTransformations[withdrawInfo.BankType];

		var transforJson = jsonTransform.Transform(bankInfo, transformation);

		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<GameWalletBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var settings = new GameWalletWithdrawSettings(
			MerchantCode: brandPaymentExtendedData.RootElement.GetProperty("merchantCode").GetString(),
			CurrencyCode: brandPaymentExtendedData.RootElement.GetProperty("currencyCode").GetString(),
			BankCode: brandPaymentExtendedData.RootElement.GetProperty("bankCode").ToString(),
			Customer: withdrawBankInfo.Detail.Name,
			Reference: formId,
			Amount: amount,
			AccountNumber: withdrawBankInfo.Detail.BankAccount,
			AccountName: withdrawBankInfo.Detail.BankName,
			MerchantSecurityCode: brandPaymentExtendedData.RootElement.GetProperty("securityCode").GetString()
		);

		return JsonSerializer.Serialize(settings, GameWalletUtility.SerializerOptions);
	}
}