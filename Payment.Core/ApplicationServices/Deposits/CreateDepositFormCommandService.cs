using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class CreateDepositFormCommandService : ICommandService<CreateDepositFormCommand>
{
	private readonly IPaymentClient _paymentClient;
	private readonly IPaymentRepository _paymentRepository;
	private readonly ITakeNumberStore _takeNumberStore;

	public CreateDepositFormCommandService(
		IPaymentRepository paymentRepository,
		IPaymentClient paymentClient,
		ITakeNumberStore takeNumberStore)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
		_takeNumberStore = takeNumberStore ?? throw new ArgumentNullException(nameof(takeNumberStore));
	}

	public async ValueTask ExecuteAsync(CreateDepositFormCommand command, CancellationToken cancellationToken = default)
	{
		var brandInfo = await _paymentRepository
			.GetBrandInfoByIdAsync(command.CreateInfo.BrandId, cancellationToken)
			.ConfigureAwait(false);

		var orderNo = await _takeNumberStore.GetDepositFormOrderNoAsync(brandInfo.Code);
		command.CreateInfo.OrderNo = orderNo;

		await _paymentRepository
			.CreateDepositFormAsync(command.FormId, command.CreateInfo, cancellationToken)
			.ConfigureAwait(false);

		await _paymentRepository
			.SubmitDepositFormStatusAsync(
				command.FormId,
				PaymentFlowState.WaitPayment,
				null,
				null,
				null,
				null,
				cancellationToken)
			.ConfigureAwait(false);

		await _paymentClient
			.EmitDepositFormCreatedEventAsync(command.FormId, command.CreateInfo.BrandId)
			.ConfigureAwait(false);
	}
}
