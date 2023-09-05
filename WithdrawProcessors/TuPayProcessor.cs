using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.Payment.TuPay;

namespace Tgs.Payment.WithdrawProcessors;

internal class TuPayProcessor : IWithdrawProcessor
{
	private readonly ILogger<TuPayProcessor> _logger;
	private readonly TuPaySerializer _tuPaySerializer;
	public string Subject => "tgs.payment.tupay.withdraw";

	public TuPayProcessor(
		TuPaySerializer tuPaySerializer,
		ILogger<TuPayProcessor> logger)
	{
		_tuPaySerializer = tuPaySerializer ?? throw new ArgumentNullException(nameof(tuPaySerializer));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<TuPayBankInfo>>(bankInfo, PaymentUtility.SerializerOptions);
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		_logger.LogInformation(
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

		return _tuPaySerializer.Serialize(settings);
	}
}
