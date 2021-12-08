using System;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.ApplePay;

namespace Tgs.Payment.WithdrawProcessors
{
	public class ApplePayProcessor : IWithdrawProcessor
	{
		private readonly ApplePaySerializer _applePaySerializer;
		private readonly JsonSerializerOptions _jsonSerializerOptions;
		public string Subject => "tgs.payment.applepay.withdraw";

		public ApplePayProcessor(
			IOptions<PaymentOptions> jsonOptionsAccessor,
			ApplePaySerializer applePaySerializer)
		{
			_jsonSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;
			_applePaySerializer = applePaySerializer ?? throw new ArgumentNullException(nameof(applePaySerializer));
		}

		public string MakeWithdrawSettings(
			Guid formId,
			decimal amount,
			string bankInfo,
			BrandPaymentInfo brandPayment)
		{
			var withdrawBankInfo = JsonSerializer
				.Deserialize<WithdrawBankInfo<ApplePayBankInfo>>(bankInfo, _jsonSerializerOptions);

			var brandPaymentExtendedData = JsonSerializer
				.Deserialize<JsonDocument>(brandPayment.ExtendedData, _jsonSerializerOptions);

			var settings = new ApplePayWithdrawSettings(
				brandPaymentExtendedData.RootElement.GetProperty("apiKey").GetString(),
				brandPaymentExtendedData.RootElement.GetProperty("storeGuid").GetString(),
				formId,
				brandPaymentExtendedData.RootElement.GetProperty("type").GetString(),
				amount,
				withdrawBankInfo.Detail.Name,
				withdrawBankInfo.Detail.BankName,
				withdrawBankInfo.Detail.BankAccount);

			return _applePaySerializer.Serialize(settings);
		}
	}
}