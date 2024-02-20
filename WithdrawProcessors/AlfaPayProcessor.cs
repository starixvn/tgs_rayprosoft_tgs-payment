using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.Payment.AlfaPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class AlfaPayProcessor(
	ILogger<AlfaPayProcessor> logger)
	: IWithdrawProcessor
{
	public string Subject => "tgs.payment.alfapay.withdraw";

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<AlfaPayBankInfo>>(bankInfo, PaymentUtility.SerializerOptions);
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		logger.LogInformation(
			"{processor} formId:{formId}, bankInfo:{bankInfo}, brandPayment ExtendedData:{brandPaymentExtendedData}",
			nameof(AlfaPayProcessor),
			formId,
			bankInfo,
			brandPayment.ExtendedData);

		var settings = new AlfaPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("apiToken").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("notifyToken").GetString(),
			withdrawBankInfo.Detail.AccountNumber.Trim(),
			brandPaymentExtendedData.RootElement.GetProperty("bankId").GetString(),
			withdrawBankInfo.Detail.BankOwner.Trim(),
			formId);

		return JsonSerializer.Serialize(settings, AlfaPayUtility.SerializerOptions);
	}
}
