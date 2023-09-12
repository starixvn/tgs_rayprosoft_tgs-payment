using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.ApplicationServices.BankInfos;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.Payment.MessageQueue;

internal class RebuildPlayerBankInfoHashHandler : IMessageHandler
{
	private readonly ICommandService<RebuildPlayerBankInfoHashCommand> _rebuildService;

	public RebuildPlayerBankInfoHashHandler(ICommandService<RebuildPlayerBankInfoHashCommand> rebuildService)
	{
		_rebuildService = rebuildService ?? throw new ArgumentNullException(nameof(rebuildService));
	}

	public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = PlayerBankInfoChangedEvent.Parser.ParseFrom(data.Span);

		var command = new RebuildPlayerBankInfoHashCommand(request.Id);

		return _rebuildService.ExecuteAsync(command, cancellationToken);
	}
}
