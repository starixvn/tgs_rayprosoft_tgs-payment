using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.Payment.TuPay;

namespace Tgs.Payment.WithdrawProcessors;

internal class TuPayProcessor(
	TuPaySerializer tuPaySerializer,
	ILogger<TuPayProcessor> logger) : IWithdrawProcessor
{
	public string Subject => "tgs.payment.tupay.withdraw";

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<TuPayBankInfo>>(bankInfo, PaymentUtility.SerializerOptions);
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		logger.LogInformation(
			"TuPayProcessor formId:{formId}, bankInfo:{bankInfo}, brandPayment ExtendedData:{brandPayment.ExtendedData}",
			formId,
			bankInfo,
			brandPayment.ExtendedData);

		var settings = new TuPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchant").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("secret").GetString(),
			brandPaymentExtendedData.RootElement.TryGetProperty("bankName", out var bankElement)
				? bankElement.GetString()
				: withdrawBankInfo.Detail.BankName,
			withdrawBankInfo.Detail.SubBankName,
			withdrawBankInfo.Detail.CardNo,
			withdrawBankInfo.Detail.CardName,
			formId,
			brandPaymentExtendedData.RootElement.TryGetProperty("remark", out var jsonElement)
				? jsonElement.GetString()
				: null);

		return tuPaySerializer.Serialize(settings);
	}
}
