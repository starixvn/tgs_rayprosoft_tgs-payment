using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.Payment.Protos;

namespace Tgs.Payment.Clients
{
	internal class PaymentClient : IPaymentClient
	{
		private readonly ILogger<PaymentClient> _logger;
		private readonly IMessageSender _messageSender;

		public PaymentClient(IMessageSender messageSender, ILogger<PaymentClient> logger)
		{
			_messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async ValueTask<string> DepositAsync(string subject, decimal amount, string? applyInfo)
		{
			var request = new DepositRequest
			{
				ApplyInfo = applyInfo ?? string.Empty,
				Amount = amount.ToString()
			};

			var result = await _messageSender
				.RequestAsync(subject, request.ToByteArray())
				.ConfigureAwait(false);

			var response = DepositResponse.Parser.ParseFrom(result);

			_logger.LogInformation(response.Result);

			return response.Result;
		}

		public async ValueTask<string> WithdrawAsync(string subject, decimal amount, string? applyInfo)
		{
			var request = new WithdrawRequest
			{
				ApplyInfo = applyInfo ?? string.Empty,
				Amount = amount.ToString()
			};

			var result = await _messageSender
				.RequestAsync(subject, request.ToByteArray())
				.ConfigureAwait(false);

			var response = WithdrawResponse.Parser.ParseFrom(result);

			_logger.LogInformation(response.Result);

			return response.Result;
		}
	}
}
