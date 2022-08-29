using System;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.SkyPay;

namespace Tgs.Payment.WithdrawProcessors;

internal class SkyPayVNProcessor : IWithdrawProcessor
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly SkyPaySerializer _skyPaySerializer;

	public string Subject { get; }

	public SkyPayVNProcessor(IOptions<PaymentOptions> jsonOptionsAccessor, SkyPaySerializer skyPaySerializer)
	{
		if (jsonOptionsAccessor is null)
			throw new ArgumentNullException(nameof(jsonOptionsAccessor));

		_jsonSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;
		_skyPaySerializer = skyPaySerializer ?? throw new ArgumentNullException(nameof(skyPaySerializer));

		Subject = $"tgs.payment.skypay.vn.withdraw";
	}

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<GatewayBankInfo>>(bankInfo, _jsonSerializerOptions);

		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, _jsonSerializerOptions);

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
			.Deserialize<WithdrawBankInfo<GatewayBankInfo>>(bankInfo, _jsonSerializerOptions);

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
			.Deserialize<WithdrawBankInfo<MomoBankInfo>>(bankInfo, _jsonSerializerOptions);

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
			.Deserialize<WithdrawBankInfo<ZaloBankInfo>>(bankInfo, _jsonSerializerOptions);

		var settings = new ZaloWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("accessToken").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("exchangeRate").GetDecimal(),
			formId,
			withdrawBankInfo.Detail.QrCode);

		return _skyPaySerializer.Serialize(settings);
	}
}
