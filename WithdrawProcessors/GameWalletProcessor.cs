using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using JSLTSharp;
using Microsoft.Extensions.DependencyInjection;
using Payment.GameWallet.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class GameWalletProcessor(JsonTransform jsonTransform, IServiceProvider serviceProvider) : IWithdrawProcessor
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
					"name": "$.detail.name",
					"bankAccount": "$.detail.bankAccount"
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
			AccountName: withdrawBankInfo.Detail.Name,
			MerchantSecurityCode: brandPaymentExtendedData.RootElement.GetProperty("securityCode").GetString()
		);

		var applyInfo = new GameWalletWithdrawApplyInfo(settings, new GameWalletWithdrawResponse(formId, string.Empty, amount));

		var applyInfoData = JsonSerializer.Serialize(applyInfo, GameWalletUtility.SerializerOptions);

		_ = Task.Run(() => SaveApplyInfoAsync(formId, applyInfoData));

		return applyInfoData;
	}

	private async Task SaveApplyInfoAsync(Guid formId, string applyInfo)
	{
		using var scope = serviceProvider.CreateScope();
		var service = scope.ServiceProvider.GetService<IPaymentRepository>();

		await service.SaveApplyInfoAsync(formId, applyInfo).ConfigureAwait(false);
	}
}