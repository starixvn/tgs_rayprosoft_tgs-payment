using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.Payment.MessageQueue;

internal class AutoApproveWithdrawHandler : IMessageHandler
{
	private readonly ICommandService<AutoApproveWithdtawFormCommand> _commandService;

	public AutoApproveWithdrawHandler(ICommandService<AutoApproveWithdtawFormCommand> commandService)
	{
		_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
	}

	public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = FormEvent.Parser.ParseFrom(data.Span);

		var command = new AutoApproveWithdtawFormCommand(
			Guid.Parse(request.BrandId),
			Guid.Parse(request.FormId));

		return _commandService.ExecuteAsync(command, cancellationToken);
	}
}
