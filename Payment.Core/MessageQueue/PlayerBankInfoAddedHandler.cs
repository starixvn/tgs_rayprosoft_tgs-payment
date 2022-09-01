using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.MessageQueue;
using Tgs.Payment.ApplicationServices.BankInfos;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;

namespace Tgs.Payment.MessageQueue;

internal class PlayerBankInfoAddedHandler : IMessageHandler
{
	private readonly ICommandService<ProcessBankInfoTagsCommand> _processTagsService;

	public PlayerBankInfoAddedHandler(ICommandService<ProcessBankInfoTagsCommand> processTagsService)
	{
		_processTagsService = processTagsService ?? throw new ArgumentNullException(nameof(processTagsService));
	}

	public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = PlayerBankInfoChangedEvent.Parser.ParseFrom(data.Span);

		var command = new ProcessBankInfoTagsCommand(request.Id);

		return _processTagsService.ExecuteAsync(command, cancellationToken);
	}
}
