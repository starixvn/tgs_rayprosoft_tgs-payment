using System;
using System.Text.Json;
using Tgs.Payment.ApplePay;

namespace Tgs.Payment.WithdrawProcessors
{
	public class ApplePayProcessor : IWithdrawProcessor
	{
		private readonly ApplePaySerializer _applePaySerializer;

		public string Subject => "tgs.payment.applepay.withdraw";

		public ApplePayProcessor(ApplePaySerializer applePaySerializer)
		{
			_applePaySerializer = applePaySerializer ?? throw new ArgumentNullException(nameof(applePaySerializer));
		}

		public string MakeWithdrawSettings(
			Guid formId,
			decimal amount,
			string bankInfo,
			BrandPaymentInfo brandPayment)
		{
			var withdrawBankInfo = _applePaySerializer.Deserialize<WithdrawBankInfo<ApplePayBankInfo>>(bankInfo);

			var brandPaymentExtendedData = _applePaySerializer.Deserialize<JsonDocument>(brandPayment.ExtendedData);

			var settings = new ApplePayWithdrawSettings(
				brandPaymentExtendedData.RootElement.GetProperty("apiKey").GetString(),
				brandPaymentExtendedData.RootElement.GetProperty("storeGuid").GetString(),
				formId,
				brandPaymentExtendedData.RootElement.GetProperty("type").GetString(),
				amount,
				withdrawBankInfo.Detail.Name,
				withdrawBankInfo.Detail.BankName,
				withdrawBankInfo.Detail.BankAccount
				);

			return _applePaySerializer.Serialize(settings);
		}
	}
}
