using System;
using System.Text.Json;
using Tgs.Payment.SkyPay;

namespace Tgs.Payment.WithdrawProcessors;

internal class SkyPayVNProcessor : IWithdrawProcessor
{
	private readonly SkyPaySerializer _skyPaySerializer;

	public string Subject { get; }

	public SkyPayVNProcessor(
		SkyPaySerializer skyPaySerializer)
	{
		_skyPaySerializer = skyPaySerializer ?? throw new ArgumentNullException(nameof(skyPaySerializer));

		Subject = $"tgs.payment.skypay.vn.withdraw";
	}

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<GatewayBankInfo>>(bankInfo, PaymentUtility.SerializerOptions);

		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		return withdrawBankInfo.BankType switch
		{
			"Bank" => MakeBankWithdrawSettings(formId, bankInfo, brandPaymentExtendedData),
			"Momo" => MakeMomoWithdrawSettings(formId, bankInfo, brandPaymentExtendedData),
			"Zalo" => MakeZaloWithdrawSettings(formId, bankInfo, brandPaymentExtendedData),
			_ => throw new NotSupportedException(),
		};
	}

	private string MakeBankWithdrawSettings(Guid formId, string bankInfo, JsonDocument brandPaymentExtendedData)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<GatewayBankInfo>>(bankInfo, PaymentUtility.SerializerOptions);

		var settings = new GatewayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("accessToken").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("exchangeRate").GetDecimal(),
			formId,
			brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString(),
			withdrawBankInfo.Detail.AccountNumber,
			withdrawBankInfo.Detail.RealName);

		return _skyPaySerializer.Serialize(settings);
	}

	private string MakeMomoWithdrawSettings(Guid formId, string bankInfo, JsonDocument brandPaymentExtendedData)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<MomoBankInfo>>(bankInfo, PaymentUtility.SerializerOptions);

		var settings = new MomoWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("accessToken").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("exchangeRate").GetDecimal(),
			formId,
			withdrawBankInfo.Detail.QrCode);

		return _skyPaySerializer.Serialize(settings);
	}

	private string MakeZaloWithdrawSettings(Guid formId, string bankInfo, JsonDocument brandPaymentExtendedData)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<ZaloBankInfo>>(bankInfo, PaymentUtility.SerializerOptions);

		var settings = new ZaloWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("accessToken").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("exchangeRate").GetDecimal(),
			formId,
			withdrawBankInfo.Detail.QrCode);

		return _skyPaySerializer.Serialize(settings);
	}
}
