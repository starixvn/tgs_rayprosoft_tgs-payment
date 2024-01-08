using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.ApplicationServices.BankInfos;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.Payment.MessageQueue;

internal class RebuildPlayerBankInfoHashHandler(ICommandService<RebuildPlayerBankInfoHashCommand> rebuildService)
	: IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(
		string subject,
		ReadOnlyMemory<byte> data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		var request = PlayerBankInfoChangedEvent.Parser.ParseFrom(data.Span);

		var command = new RebuildPlayerBankInfoHashCommand(request.Id);

		return rebuildService.ExecuteAsync(command, cancellationToken);
	}
}
