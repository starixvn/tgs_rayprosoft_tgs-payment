using System;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.LeePay;
using TGS.Core.Infrastructures.Json;

namespace Tgs.Payment.WithdrawProcessors
{
	public class LeePayProcessor : IWithdrawProcessor
	{
		private readonly IJsonSerializationProvider _jsonSerializationProvider;
		private readonly JsonSerializerOptions _jsonSerializerOptions;
		private readonly LeePaySerializer _leePaySerializer;
		public string Subject => "tgs.payment.leepay.withdraw";

		public LeePayProcessor(
			IJsonSerializationProvider jsonSerializationProvider,
			IOptions<PaymentOptions> jsonOptionsAccessor,
			LeePaySerializer leePaySerializer)
		{
			_jsonSerializationProvider = jsonSerializationProvider ?? throw new ArgumentNullException(nameof(jsonSerializationProvider));
			_leePaySerializer = leePaySerializer ?? throw new ArgumentNullException(nameof(leePaySerializer));
			_jsonSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;
		}

		public string MakeWithdrawSettings(
			Guid formId,
			decimal amount,
			string bankInfo,
			BrandPaymentInfo brandPayment)
		{
			var withdrawBankInfo = _jsonSerializationProvider
				.DeserializeFromString<WithdrawBankInfo<LeePayBankInfo>>(bankInfo, _jsonSerializerOptions);
			var destinationAddress = withdrawBankInfo.Detail.Address;

			var brandPaymentExtendedData = _jsonSerializationProvider
				.DeserializeFromString<JsonDocument>(brandPayment.ExtendedData);

			var settings = new LeePayWithdrawSettings(
				brandPaymentExtendedData.RootElement.GetProperty("systemId").GetString(),
				brandPaymentExtendedData.RootElement.GetProperty("systemKey").GetString(),
				formId,
				destinationAddress,
				brandPaymentExtendedData.RootElement.GetProperty("channelCurrency").GetString(),
				brandPaymentExtendedData.RootElement.GetProperty("channel").GetString());

			return _leePaySerializer.Serialize(settings);
		}
	}
}
