using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class DeletePaymentMethodChannelCommandService : ICommandService<DeletePaymentMethodChannelCommand>
{
	private readonly IPaymentClient _paymentClient;
	private readonly IPaymentRepository _paymentRepository;

	public DeletePaymentMethodChannelCommandService(IPaymentRepository paymentRepository, IPaymentClient paymentClient)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
	}

	public async ValueTask ExecuteAsync(DeletePaymentMethodChannelCommand command, CancellationToken cancellationToken = default)
	{
		await _paymentRepository
			.DeletePaymentMethodChannelAsync(command.PaymentMethodChannelId, cancellationToken)
			.ConfigureAwait(false);

		await _paymentClient
			.EmitPaymentMethodChannelChangedEventAsync(command.PaymentMethodChannelId, cancellationToken)
			.ConfigureAwait(false);
	}
}
