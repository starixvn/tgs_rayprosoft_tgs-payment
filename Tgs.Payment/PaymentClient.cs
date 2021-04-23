using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.Payment.Protos;

namespace Tgs.Payment
{
	internal class PaymentClient : IPaymentClient
	{
		private readonly IMessageSender _messageSender;
		private readonly ILogger<PaymentClient> _logger;

		public PaymentClient(IMessageSender messageSender, ILogger<PaymentClient> logger)
		{
			_messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async ValueTask<string> DepositAsync(string subject, decimal amount, string applyInfo)
		{
			var request = new DepositRequest
			{
				ApplyInfo = applyInfo,
				Amount = amount.ToString()
			};

			var result = await _messageSender
				.RequestAsync(subject, request.ToByteArray())
				.ConfigureAwait(false);

			var response = DepositResponse.Parser.ParseFrom(result);

			_logger.LogInformation(response.Result);

			return response.Result;
		}
	}
}
