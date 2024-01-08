using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.Payment.ZeroNineNinePay;

namespace Tgs.Payment.WithdrawProcessors;

internal class ZeroNineNinePayProcessor(
	ZeroNineNinePaySerializer zeroNineNinePaySerializer,
	ILogger<ZeroNineNinePayProcessor> logger) : IWithdrawProcessor
{
	public string Subject => "tgs.payment.zeronineninepay.withdraw";

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<ZeroNineNinePayBankInfo>>(bankInfo, PaymentUtility.SerializerOptions);

		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		logger.LogInformation(
			"ZeroNineNinePayProcessor formId:{formId}, bankInfo:{bankInfo}, brandPayment ExtendedData:{brandPayment.ExtendedData}",
			formId,
			bankInfo,
			brandPayment.ExtendedData);

		var settings = new ZeroNineNinePayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchantId").GetInt32(),
			brandPaymentExtendedData.RootElement.GetProperty("merchantKey").GetString(),
			withdrawBankInfo.Detail.Name,
			brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString(),
			withdrawBankInfo.Detail.BankAccount,
			withdrawBankInfo.Detail.BankBranch,
			formId);

		return zeroNineNinePaySerializer.Serialize(settings);
	}
}
