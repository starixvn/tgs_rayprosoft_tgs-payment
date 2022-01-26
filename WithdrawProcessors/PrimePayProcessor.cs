using System;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.PrimePay;

namespace Tgs.Payment.WithdrawProcessors;

public class PrimePayProcessor : IWithdrawProcessor
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly PrimePaySerializer _primePaySerializer;

	public string Subject => "tgs.payment.primepay.withdraw";

	public PrimePayProcessor(
		IOptions<PaymentOptions> jsonOptionsAccessor,
		PrimePaySerializer primePaySerializer)
	{
		_jsonSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;
		_primePaySerializer = primePaySerializer ?? throw new ArgumentNullException(nameof(primePaySerializer));
	}

	public string MakeWithdrawSettings(
		Guid formId,
		decimal amount,
		string bankInfo,
		BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<PrimePayBankInfo>>(bankInfo, _jsonSerializerOptions);

		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, _jsonSerializerOptions);

		var settings = new PrimePayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchantId").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("key").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("iv").GetString(),
			formId,
			brandPaymentExtendedData.RootElement.GetProperty("currency").GetString(),
			withdrawBankInfo.Detail.BankAccount,
			withdrawBankInfo.Detail.Name,
			withdrawBankInfo.Detail.BankBranch,
			withdrawBankInfo.Detail.BankName);

		return _primePaySerializer.Serialize(settings);
	}
}
