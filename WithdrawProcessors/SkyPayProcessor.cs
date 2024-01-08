using System;
using System.Text.Json;
using Tgs.Payment.SkyPay;

namespace Tgs.Payment.WithdrawProcessors;

internal class SkyPayProcessor(
	SkyPaySerializer skyPaySerializer) : IWithdrawProcessor
{
	public string Subject { get; } = $"tgs.payment.skypay.cn.withdraw";

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<GatewayBankInfo>>(bankInfo, PaymentUtility.SerializerOptions);

		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		return withdrawBankInfo.BankType switch
		{
			"Bank" => MakeBankWithdrawSettings(formId, bankInfo, brandPaymentExtendedData),
			_ => throw new NotSupportedException(),
		};
	}

	private string MakeBankWithdrawSettings(Guid formId, string bankInfo, JsonDocument brandPaymentExtendedData)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<GatewayBankInfo>>(bankInfo, PaymentUtility.SerializerOptions);

		var settings = new GatewayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("accessToken").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("exchangeRate").GetDecimal(),
			formId,
			brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString(),
			withdrawBankInfo.Detail.AccountNumber,
			withdrawBankInfo.Detail.RealName);

		return skyPaySerializer.Serialize(settings);
	}
}
