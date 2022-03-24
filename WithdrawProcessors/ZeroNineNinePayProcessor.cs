using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.Payment.ZeroNineNinePay;

namespace Tgs.Payment.WithdrawProcessors;

public class ZeroNineNinePayProcessor : IWithdrawProcessor
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly ILogger<ZeroNineNinePayProcessor> _logger;
	private readonly ZeroNineNinePaySerializer _zeroNineNinePaySerializer;
	public string Subject => "tgs.payment.zeronineninepay.withdraw";

	public ZeroNineNinePayProcessor(
		IOptions<PaymentOptions> jsonOptionsAccessor,
		ZeroNineNinePaySerializer zeroNineNinePaySerializer,
		ILogger<ZeroNineNinePayProcessor> logger)
	{
		if (jsonOptionsAccessor is null)
			throw new ArgumentNullException(nameof(jsonOptionsAccessor));

		_jsonSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;
		_zeroNineNinePaySerializer = zeroNineNinePaySerializer;
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
		.Deserialize<WithdrawBankInfo<ZeroNineNinePayBankInfo>>(bankInfo, _jsonSerializerOptions);

		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, _jsonSerializerOptions);

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
