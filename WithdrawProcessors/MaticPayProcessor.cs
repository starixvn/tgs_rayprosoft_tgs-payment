using System;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.MaticPay;

namespace Tgs.Payment.WithdrawProcessors
{
	public class MaticPayProcessor : IWithdrawProcessor
	{
		private readonly JsonSerializerOptions _jsonSerializerOptions;
		private readonly MaticPaySerializer _maticPaySerializer;

		public string Subject => "tgs.payment.maticpay.withdraw";

		public MaticPayProcessor(
			IOptions<PaymentOptions> jsonOptionsAccessor,
			MaticPaySerializer maticPaySerializer)
		{
			_jsonSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;
			_maticPaySerializer = maticPaySerializer ?? throw new ArgumentNullException(nameof(maticPaySerializer));
		}

		public string MakeWithdrawSettings(
			Guid formId,
			decimal amount,
			string bankInfo,
			BrandPaymentInfo brandPayment)
		{
			var withdrawBankInfo = JsonSerializer
				.Deserialize<WithdrawBankInfo<MaticPayBankInfo>>(bankInfo, _jsonSerializerOptions);

			var brandPaymentExtendedData = JsonSerializer
				.Deserialize<JsonDocument>(brandPayment.ExtendedData, _jsonSerializerOptions);

			var settings = new MaticPayWithdrawSettings(
				brandPaymentExtendedData.RootElement.GetProperty("merchantNo").GetString(),
				brandPaymentExtendedData.RootElement.GetProperty("apiKey").GetString(),
				formId,
				amount,
				withdrawBankInfo.Detail.AccountName,
				brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString(),
				withdrawBankInfo.Detail.BankBranchName,
				withdrawBankInfo.Detail.CardNo
				);

			return _maticPaySerializer.Serialize(settings);
		}
	}
}