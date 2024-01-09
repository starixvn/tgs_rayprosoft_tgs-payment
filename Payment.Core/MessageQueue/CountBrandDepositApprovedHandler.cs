using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.Payment.ApplicationServices.Deposits;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.Payment.MessageQueue;

internal class CountBrandDepositApprovedHandler(
	ICommandService<CountDepositApprovedCommand> commandService,
	ILogger<CountBrandDepositApprovedHandler> logger)
	: IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(
		string subject,
		ReadOnlyMemory<byte> data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		var request = FormEventEx.Parser.ParseFrom(data.Span);

		logger.LogInformation("Count Brand Deposit Form, BrandId:{brandId}, FormId:{formId}, Status:{status}",
			request.BrandId.ToGuid(),
			request.FormId.ToGuid(),
			request.Status);

		var command = new CountDepositApprovedCommand(
			request.BrandId.ToGuid(),
			request.FormId.ToGuid(),
			Enum.Parse<PaymentFlowState>(request.Status));

		return commandService.ExecuteAsync(command, cancellationToken);
	}
}
