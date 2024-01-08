using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.ApplicationServices.BankInfos;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.Payment.MessageQueue;

internal class PlayerBankInfoAddedHandler(ICommandService<ProcessBankInfoTagsCommand> processTagsService) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		var request = PlayerBankInfoChangedEvent.Parser.ParseFrom(data.Span);

		var command = new ProcessBankInfoTagsCommand(request.Id);

		return processTagsService.ExecuteAsync(command, cancellationToken);
	}
}
