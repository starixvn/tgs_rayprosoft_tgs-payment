using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.Payment.ZeroNineNinePay;

namespace Tgs.Payment.WithdrawProcessors;

internal class ZeroNineNinePayProcessor : IWithdrawProcessor
{
	private readonly ILogger<ZeroNineNinePayProcessor> _logger;
	private readonly ZeroNineNinePaySerializer _zeroNineNinePaySerializer;
	public string Subject => "tgs.payment.zeronineninepay.withdraw";

	public ZeroNineNinePayProcessor(
		ZeroNineNinePaySerializer zeroNineNinePaySerializer,
		ILogger<ZeroNineNinePayProcessor> logger)
	{
		_zeroNineNinePaySerializer = zeroNineNinePaySerializer;
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<ZeroNineNinePayBankInfo>>(bankInfo, PaymentUtility.SerializerOptions);

		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		_logger.LogInformation(
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

		return _zeroNineNinePaySerializer.Serialize(settings);
	}
}
