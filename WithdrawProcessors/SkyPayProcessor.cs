using System;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.ApplePay;
using Tgs.Payment.SkyPay;

namespace Tgs.Payment.WithdrawProcessors
{
	internal class SkyPayProcessor : IWithdrawProcessor
	{
		private readonly JsonSerializerOptions _jsonSerializerOptions;
		private readonly SkyPaySerializer _skyPaySerializer;

		public string Subject => "tgs.payment.skypay.withdraw";

		public SkyPayProcessor(IOptions<PaymentOptions> jsonOptionsAccessor, SkyPaySerializer skyPaySerializer)
		{
			if (jsonOptionsAccessor is null)
				throw new ArgumentNullException(nameof(jsonOptionsAccessor));

			_jsonSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;
			_skyPaySerializer = skyPaySerializer ?? throw new ArgumentNullException(nameof(skyPaySerializer));
		}

		public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
		{
			var withdrawBankInfo = JsonSerializer
				.Deserialize<WithdrawBankInfo<ApplePayBankInfo>>(bankInfo, _jsonSerializerOptions);

			var brandPaymentExtendedData = JsonSerializer
				.Deserialize<JsonDocument>(brandPayment.ExtendedData, _jsonSerializerOptions);

			switch (withdrawBankInfo.BankType)
			{
				case "Bank":
					return MakeBankWithdrawSettings(formId, bankInfo, brandPaymentExtendedData);

				case "Momo":
					return MakeMomoWithdrawSettings(formId, bankInfo, brandPaymentExtendedData);

				case "Zalo":
					return MakeZaloWithdrawSettings(formId, bankInfo, brandPaymentExtendedData);

				default:
					throw new NotSupportedException();
			}
		}

		private string MakeBankWithdrawSettings(Guid formId, string bankInfo, JsonDocument brandPaymentExtendedData)
		{
			var withdrawBankInfo = JsonSerializer
				.Deserialize<WithdrawBankInfo<GatewayBankInfo>>(bankInfo, _jsonSerializerOptions);

			var settings = new GatewayWithdrawSettings(
				brandPaymentExtendedData.RootElement.GetProperty("accessToken").GetString(),
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
				formId,
				withdrawBankInfo.Detail.QrCode);

			return _skyPaySerializer.Serialize(settings);
		}
	}
}
