using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.Payment.MessageQueue;

internal class SaveWithdrawThirdNoHandler(
	ICommandService<SubmitWithdrawFormStatusCommand> commandService,
	ILogger<SaveWithdrawThirdNoHandler> logger)
	: IMessageHandler<ReadOnlyMemory<byte>>
{

	public async ValueTask HandleAsync(
		string subject,
		ReadOnlyMemory<byte> data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Receive SaveWithdrawThirdNo request.");

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

		await commandService
			.ExecuteAsync(command, cancellationToken)
			.ConfigureAwait(false);

		logger.LogInformation("SaveWithdrawThirdNo processing completed.");
	}
}
