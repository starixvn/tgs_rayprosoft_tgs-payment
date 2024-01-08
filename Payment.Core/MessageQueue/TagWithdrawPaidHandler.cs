using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.Payment.MessageQueue;

internal class TagWithdrawPaidHandler(
	ICommandService<SubmitWithdrawFormStatusCommand> submitFormStatusService)
	: IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(
		string subject,
		ReadOnlyMemory<byte> data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		var request = TagPaidRequest.Parser.ParseFrom(data.Span);

		var command = new SubmitWithdrawFormStatusCommand(
			request.FormId,
			PaymentFlowState.Paid,
			null,
			null,
			null,
			null,
			null);

		return submitFormStatusService.ExecuteAsync(command, cancellationToken);
	}
}
