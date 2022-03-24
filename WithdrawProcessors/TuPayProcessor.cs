using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.Payment.TuPay;

namespace Tgs.Payment.WithdrawProcessors;

internal class TuPayProcessor : IWithdrawProcessor
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly ILogger<TuPayProcessor> _logger;
	private readonly TuPaySerializer _tuPaySerializer;
	public string Subject => "tgs.payment.tupay.withdraw";

	public TuPayProcessor(IOptions<PaymentOptions> jsonOptionsAccessor, TuPaySerializer tuPaySerializer, ILogger<TuPayProcessor> logger)
	{
		if (jsonOptionsAccessor is null)
			throw new ArgumentNullException(nameof(jsonOptionsAccessor));

		_jsonSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;
		_tuPaySerializer = tuPaySerializer ?? throw new ArgumentNullException(nameof(tuPaySerializer));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<TuPayBankInfo>>(bankInfo, _jsonSerializerOptions);
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, _jsonSerializerOptions);

		_logger.LogInformation(
			"TuPayProcessor formId:{formId}, bankInfo:{bankInfo}, brandPayment ExtendedData:{brandPayment.ExtendedData}",
			formId,
			bankInfo,
			brandPayment.ExtendedData);

		var settings = new TuPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchant").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("secret").GetString(),
			withdrawBankInfo.Detail.BankName,
			withdrawBankInfo.Detail.SubBankName,
			withdrawBankInfo.Detail.CardNo,
			withdrawBankInfo.Detail.CardName,
			formId,
			brandPaymentExtendedData.RootElement.GetProperty("remark").GetString());

		return _tuPaySerializer.Serialize(settings);
	}
}
