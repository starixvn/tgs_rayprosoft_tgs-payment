using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.Payment.MessageQueue;

internal class AutoApproveWithdrawHandler : IMessageHandler
{
	private readonly ICommandService<AutoApproveWithdtawFormCommand> _commandService;
	private readonly ILogger<AutoApproveWithdrawHandler> _logger;

	public AutoApproveWithdrawHandler(
		ICommandService<AutoApproveWithdtawFormCommand> commandService,
		ILogger<AutoApproveWithdrawHandler> logger)
	{
		_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = FormEventEx.Parser.ParseFrom(data.Span);

		_logger.LogInformation(
			"AutoApproveWithdrawHandler received: brandId:{brandId}, formId:{formId}, status:{status}",
			request.BrandId.ToGuid(),
			request.FormId.ToGuid(),
			request.Status);

		var command = new AutoApproveWithdtawFormCommand(
			request.BrandId.ToGuid(),
			request.FormId.ToGuid(),
			Enum.Parse<PaymentFlowState>(request.Status));

		return _commandService.ExecuteAsync(command, cancellationToken);
	}
}
