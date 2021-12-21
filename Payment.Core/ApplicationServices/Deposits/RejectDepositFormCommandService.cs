using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits
{
	internal class RejectDepositFormCommandService : ICommandService<RejectDepositFormCommand>
	{
		private readonly ILogger<RejectDepositFormCommandService> _logger;
		private readonly IMessageSender _messageSender;

		public RejectDepositFormCommandService(
			IMessageSender messageSender,
			ILogger<RejectDepositFormCommandService> logger)
		{
			_messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public ValueTask ExecuteAsync(RejectDepositFormCommand command, CancellationToken cancellationToken = default)
		{
			var request = new RejectDepositRequest
			{
				FormId = command.FormId,
				BrandId = command.BrandId
			};

			_logger.LogInformation($"Send RejectDepositRequest, FormId:{command.FormId}, BrandId:{command.BrandId}");

			return _messageSender
				.SendAsync("tgs.payment.reject", request.ToByteArray());
		}
	}
}
