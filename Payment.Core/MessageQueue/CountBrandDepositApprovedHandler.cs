using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.Payment.ApplicationServices.Deposits;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;

namespace Tgs.Payment.MessageQueue;

internal class CountBrandDepositApprovedHandler : IMessageHandler
{
	private readonly ICommandService<CountDepositApprovedCommand> _commandService;
	private readonly ILogger<CountBrandDepositApprovedHandler> _logger;

	public CountBrandDepositApprovedHandler(
		ICommandService<CountDepositApprovedCommand> commandService,
		ILogger<CountBrandDepositApprovedHandler> logger)
	{
		_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = FormEventEx.Parser.ParseFrom(data.Span);

		_logger.LogInformation($"Count Brand Withdraw Paid, BrandId:{request.BrandId.ToGuid()}, FormId:{request.FormId.ToGuid()}, Status:{request.Status}");

		var command = new CountDepositApprovedCommand(
			request.BrandId.ToGuid(),
			request.FormId.ToGuid(),
			request.Status);

		return _commandService.ExecuteAsync(command, cancellationToken);
	}
}
