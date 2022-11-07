using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.Payment.AlfaPay;

namespace Tgs.Payment.WithdrawProcessors;

internal class AlfaPayProcessor : IWithdrawProcessor
{
	private readonly AlfaPaySerializer _alfaPaySerializer;
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly ILogger<AlfaPayProcessor> _logger;
	public string Subject => "tgs.payment.alfapay.withdraw";

	public AlfaPayProcessor(
		IOptions<PaymentOptions> jsonOptionsAccessor,
		AlfaPaySerializer alfaPaySerializer,
		ILogger<AlfaPayProcessor> logger)
	{
		if (jsonOptionsAccessor is null)
			throw new ArgumentNullException(nameof(jsonOptionsAccessor));

		_jsonSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;
		_alfaPaySerializer = alfaPaySerializer ?? throw new ArgumentNullException(nameof(alfaPaySerializer));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<AlfaPayBankInfo>>(bankInfo, _jsonSerializerOptions);
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, _jsonSerializerOptions);

		_logger.LogInformation(
			"{nameof(AlfaPayProcessor)} formId:{formId}, bankInfo:{bankInfo}, brandPayment ExtendedData:{brandPayment.ExtendedData}",
			nameof(AlfaPayProcessor),
			formId,
			bankInfo,
			brandPayment.ExtendedData);

		var settings = new AlfaPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("apiToken").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("notifyToken").GetString(),
			withdrawBankInfo.Detail.AccountNumber,
			brandPaymentExtendedData.RootElement.GetProperty("bankId").GetString(),
			withdrawBankInfo.Detail.BankOwner,
			formId);

		return _alfaPaySerializer.Serialize(settings);
	}
}
