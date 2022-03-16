using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.Payment.ZeroNineNinePay;

namespace Tgs.Payment.WithdrawProcessors
{
	public class ZeroNineNinePayProcessor : IWithdrawProcessor
	{
		private readonly ILogger<ZeroNineNinePayProcessor> _logger;

		public string Subject => "tgs.payment.zeronineninepay.withdraw";

		public ZeroNineNinePayProcessor(ILogger<ZeroNineNinePayProcessor> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
		{
			var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<ZeroNineNinePayBankInfo>>(bankInfo);

			var brandPaymentExtendedData = JsonSerializer
				.Deserialize<JsonDocument>(brandPayment.ExtendedData);

			_logger.LogInformation($"{brandPayment.ExtendedData}");

			var settings = new ZeroNineNinePayWithdrawSettings(
				brandPaymentExtendedData.RootElement.GetProperty("merchantId").GetInt32(),
				brandPaymentExtendedData.RootElement.GetProperty("merchantKey").GetString(),
				withdrawBankInfo.Detail.Name,
				brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString(),
				withdrawBankInfo.Detail.BankAccount,
				withdrawBankInfo.Detail.BankBranch,
				formId);

			return JsonSerializer.Serialize(settings);
		}
	}
}
