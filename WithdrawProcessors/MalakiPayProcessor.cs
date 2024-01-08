using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.Payment.MalakiPay;

namespace Tgs.Payment.WithdrawProcessors;

internal class MalakiPayProcessor(
	MalakiPaySerializer malakiPaySerializer,
	ILogger<MalakiPayProcessor> logger) : IWithdrawProcessor
{
	public string Subject => "tgs.payment.malakipay.withdraw";

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<MalakiPayBankInfo>>(bankInfo, PaymentUtility.SerializerOptions);
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		logger.LogInformation(
			"{nameof(MalakiPayProcessor)} formId:{formId}, bankInfo:{bankInfo}, brandPayment ExtendedData:{brandPayment.ExtendedData}",
			nameof(MalakiPayProcessor),
			formId,
			bankInfo,
			brandPayment.ExtendedData);

		var settings = new MalakiPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchant").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("secret").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("bankName").GetString(),
			withdrawBankInfo.Detail.SubBankName,
			withdrawBankInfo.Detail.CardNo,
			withdrawBankInfo.Detail.CardName,
			formId,
			brandPaymentExtendedData.RootElement.TryGetProperty("remark", out var jsonElement)
				? jsonElement.GetString()
				: null);

		return malakiPaySerializer.Serialize(settings);
	}
}
