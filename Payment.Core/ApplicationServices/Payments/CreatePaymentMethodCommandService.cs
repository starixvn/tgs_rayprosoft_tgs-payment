using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class CreatePaymentMethodCommandService : ICommandService<CreatePaymentMethodCommand>
{
	private readonly IPaymentClient _paymentClient;
	private readonly IPaymentRepository _paymentRepository;

	public CreatePaymentMethodCommandService(IPaymentRepository paymentRepository, IPaymentClient paymentClient)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
	}

	public async ValueTask ExecuteAsync(
		CreatePaymentMethodCommand command,
		CancellationToken cancellationToken = default)
	{
		await _paymentRepository
			.CreatePaymentMethodAsync(command.Code, command.BankType, cancellationToken)
			.ConfigureAwait(false);

		await _paymentClient
			.EmitPaymentMethodCreatedEventAsync(command.Code, cancellationToken)
			.ConfigureAwait(false);
	}
}
