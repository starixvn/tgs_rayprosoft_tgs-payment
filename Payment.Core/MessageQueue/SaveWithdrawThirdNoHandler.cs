using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.Payment.MessageQueue;

internal class SaveWithdrawThirdNoHandler : IMessageHandler
{
	private readonly ICommandService<SubmitWithdrawFormStatusCommand> _commandService;
	private readonly ILogger<SaveWithdrawThirdNoHandler> _logger;

	public SaveWithdrawThirdNoHandler(
		ICommandService<SubmitWithdrawFormStatusCommand> commandService,
		ILogger<SaveWithdrawThirdNoHandler> logger)
	{
		_commandService = commandService;
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Receive SaveWithdrawThirdNo request.");

		var request = FormThirdNoRequest.Parser.ParseFrom(data.Span);

		var formId = (Guid)request.FormId;

		var command = new SubmitWithdrawFormStatusCommand(
			formId,
			null,
			null,
			null,
			request.ThirdPartyOrderNo,
			null,
			null);

		await _commandService
			.ExecuteAsync(command, cancellationToken)
			.ConfigureAwait(false);

		_logger.LogInformation("SaveWithdrawThirdNo processing completed.");
	}
}
