using System;
using System.Text.Json;
using Tgs.Payment.LeePay;

namespace Tgs.Payment.WithdrawProcessors;

internal class LeePayProcessor(
	LeePaySerializer leePaySerializer) : IWithdrawProcessor
{
	public string Subject => "tgs.payment.leepay.withdraw";

	public string MakeWithdrawSettings(
		Guid formId,
		decimal amount,
		string bankInfo,
		BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<LeePayBankInfo>>(bankInfo, PaymentUtility.SerializerOptions);
		var destinationAddress = withdrawBankInfo.Detail.Address;

		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData);

		var settings = new LeePayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("systemId").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("systemKey").GetString(),
			formId,
			destinationAddress,
			brandPaymentExtendedData.RootElement.GetProperty("channelCurrency").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("channel").GetString());

		return leePaySerializer.Serialize(settings);
	}
}
