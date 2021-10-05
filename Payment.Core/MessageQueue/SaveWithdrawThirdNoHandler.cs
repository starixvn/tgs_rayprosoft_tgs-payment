using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.Payment.Protos;
using TGS.ServiceUtility;
using Tgs.Payment.ApplicationServices.Withdraws;

namespace Tgs.Payment.MessageQueue
{
	internal class SaveWithdrawThirdNoHandler : IMessageHandler
	{
		private readonly ICommandService<SaveWithdrawThirdPartyOrderNoCommand> _commandService;
		private readonly ILogger<SaveWithdrawThirdNoHandler> _logger;

		public SaveWithdrawThirdNoHandler(
			ICommandService<SaveWithdrawThirdPartyOrderNoCommand> commandService,
			ILogger<SaveWithdrawThirdNoHandler> logger)
		{
			_commandService = commandService;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		async ValueTask IMessageHandler.HandleAsync(byte[] data, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Receive SaveWithdrawThirdNo request.");

			var request = FormThirdNoRequest.Parser.ParseFrom(data);

			var formId = (Guid)request.FormId;

			var command = new SaveWithdrawThirdPartyOrderNoCommand(
				formId,
				request.ThirdPartyOrderNo);

			await _commandService
				.ExecuteAsync(command, cancellationToken)
				.ConfigureAwait(false);

			_logger.LogInformation("SaveWithdrawThirdNo processing completed.");
		}
	}
}