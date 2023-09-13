using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.Payment.MessageQueue;

internal class CountBrandWithdrawPaidHandler : IMessageHandler
{
	private readonly ICommandService<CountWithdrawPaidCommand> _commandService;
	private readonly ILogger<CountBrandWithdrawPaidHandler> _logger;

	public CountBrandWithdrawPaidHandler(
		ICommandService<CountWithdrawPaidCommand> commandService,
		ILogger<CountBrandWithdrawPaidHandler> logger)
	{
		_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = FormEventEx.Parser.ParseFrom(data.Span);

		_logger.LogInformation("Count Brand Withdraw Form, BrandId:{brandId}, FormId:{formId}, Status:{status}",
			request.BrandId.ToGuid(),
			request.FormId.ToGuid(),
			request.Status);

		var command = new CountWithdrawPaidCommand(
			request.BrandId.ToGuid(),
			request.FormId.ToGuid(),
			Enum.Parse<PaymentFlowState>(request.Status));

		return _commandService.ExecuteAsync(command, cancellationToken);
	}
}
