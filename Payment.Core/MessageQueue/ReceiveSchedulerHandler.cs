using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.Payment.ApplicationServices.Deposits;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;

namespace Tgs.Payment.MessageQueue
{
	internal class ReceiveSchedulerHandler : IMessageHandler
	{
		private readonly ICommandService<RejectDepositFormCommand> _commandService;
		private readonly IGetService<GetDepositFormDetailByFormIdQuery, DepositFormInfo?> _getService;
		private readonly ILogger<ReceiveSchedulerHandler> _logger;

		public ReceiveSchedulerHandler(
			ICommandService<RejectDepositFormCommand> commandService,
			IGetService<GetDepositFormDetailByFormIdQuery, DepositFormInfo?> getSerivce,
			ILogger<ReceiveSchedulerHandler> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_getService = getSerivce ?? throw new ArgumentNullException(nameof(getSerivce));
			_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
		}

		public async ValueTask HandleAsync(byte[] data, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("Receive DepositReject request From Payment Scheduler.");
			var request = ScheduleRequest.Parser.ParseFrom(data);

			var query = new GetDepositFormDetailByFormIdQuery(request.FormId);

			var depositForm = await _getService
				.GetAsync(query)
				.ConfigureAwait(false);

			if (depositForm == null)
			{
				throw new InvalidOperationException($"Can't find deposit form info. Form Id:{request.FormId}");
			}

			var command = new RejectDepositFormCommand(request.FormId, depositForm.BrandId);

			await _commandService
				.ExecuteAsync(command)
				.ConfigureAwait(false);

			_logger.LogInformation("DepositReject processing completed.");
		}
	}
}
