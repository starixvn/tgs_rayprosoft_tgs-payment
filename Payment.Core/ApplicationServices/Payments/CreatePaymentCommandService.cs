using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class CreatePaymentCommandService : ICommandService<CreatePaymentCommand>
{
	private readonly IPaymentClient _paymentClient;
	private readonly IPaymentRepository _paymentRepository;

	public CreatePaymentCommandService(IPaymentRepository paymentRepository, IPaymentClient paymentClient)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
	}

	public async ValueTask ExecuteAsync(
		CreatePaymentCommand command,
		CancellationToken cancellationToken = default)
	{
		await _paymentRepository.CreatePaymentAsync(
			new PaymentCreateInfo(
				command.Code,
				command.Description,
				command.ExtendedData),
			cancellationToken).ConfigureAwait(false);

		await _paymentClient
			.EmitPaymentCreatedEventAsync(command.Code, cancellationToken)
			.ConfigureAwait(false);
	}
}
