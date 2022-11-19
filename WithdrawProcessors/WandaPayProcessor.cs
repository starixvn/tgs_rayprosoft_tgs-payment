using System;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.WandaPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class WandaPayProcessor : IWithdrawProcessor
{
	private readonly JsonSerializerOptions _paymentSerializerOptions;

	public string Subject => "tgs.payment.wandapay.withdraw";

	public WandaPayProcessor(IOptions<PaymentOptions> jsonOptionsAccessor)
	{
		_paymentSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;
	}

	public string MakeWithdrawSettings(
		Guid formId,
		decimal amount,
		string bankInfo,
		BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<WandaPayBankInfo>>(bankInfo, _paymentSerializerOptions);
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, _paymentSerializerOptions);

		var settings = new WandaPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("storeCode").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("signKey").GetString(),
			formId,
			withdrawBankInfo.Detail.AccountName,
			withdrawBankInfo.Detail.AccountNumber,
			brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString());

		return JsonSerializer.Serialize(settings, WandaPayUtility.SerializerOptions);
	}
}
