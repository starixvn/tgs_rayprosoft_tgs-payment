using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.Payment.MessageQueue;

internal class TagWithdrawPaidHandler : IMessageHandler
{
	private readonly ICommandService<SubmitWithdrawFormStatusCommand> _submitFormStatusService;

	public TagWithdrawPaidHandler(
		ICommandService<SubmitWithdrawFormStatusCommand> submitFormStatusService)
	{
		_submitFormStatusService = submitFormStatusService ?? throw new ArgumentNullException(nameof(submitFormStatusService));
	}

	public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
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

		return _submitFormStatusService.ExecuteAsync(command, cancellationToken);
	}
}
