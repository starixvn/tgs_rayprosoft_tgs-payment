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
	internal class SaveWithdrawThirdNoProcessor : IMessageHandler
	{
		private readonly ICommandService<SaveWithdrawThirdPartyOrderNoCommand> _commandService;
		private readonly ILogger<SaveWithdrawThirdNoProcessor> _logger;

		public SaveWithdrawThirdNoProcessor(
			ICommandService<SaveWithdrawThirdPartyOrderNoCommand> commandService,
			ILogger<SaveWithdrawThirdNoProcessor> logger)
		{
			_commandService = commandService;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		async ValueTask IMessageHandler.HandleAsync(byte[] data, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Receive SaveWithdrawThirdNo request.");

			var request = FormThirdNoEvent.Parser.ParseFrom(data);

			var formId = Guid.Parse(request.FormId);

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