using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.Payment.AlfaPay;

namespace Tgs.Payment.WithdrawProcessors;

internal class AlfaPayProcessor : IWithdrawProcessor
{
	private readonly AlfaPaySerializer _alfaPaySerializer;
	private readonly ILogger<AlfaPayProcessor> _logger;
	public string Subject => "tgs.payment.alfapay.withdraw";

	public AlfaPayProcessor(
		AlfaPaySerializer alfaPaySerializer,
		ILogger<AlfaPayProcessor> logger)
	{
		_alfaPaySerializer = alfaPaySerializer ?? throw new ArgumentNullException(nameof(alfaPaySerializer));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<AlfaPayBankInfo>>(bankInfo, PaymentUtility.SerializerOptions);
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		_logger.LogInformation(
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

		return _alfaPaySerializer.Serialize(settings);
	}
}
