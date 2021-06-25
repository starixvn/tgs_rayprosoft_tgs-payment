using System;
using System.Text.Json;
using Tgs.Payment.PrimePay;

namespace Tgs.Payment.WithdrawProcessors
{
	public class PrimePayProcessor : IWithdrawProcessor
	{
		private readonly PrimePaySerializer _primePaySerializer;

		public string Subject => "tgs.payment.primepay.withdraw";

		public PrimePayProcessor(PrimePaySerializer primePaySerializer)
		{
			_primePaySerializer = primePaySerializer ?? throw new ArgumentNullException(nameof(primePaySerializer));
		}

		public string MakeWithdrawSettings(
			Guid formId,
			decimal amount,
			string bankInfo,
			BrandPaymentInfo brandPayment)
		{
			var withdrawBankInfo = _primePaySerializer.Deserialize<WithdrawBankInfo<PrimePayBankInfo>>(bankInfo);

			var brandPaymentExtendedData = _primePaySerializer.Deserialize<JsonDocument>(brandPayment.ExtendedData);

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
}
