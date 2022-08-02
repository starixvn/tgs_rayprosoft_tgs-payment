using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class DeletePaymentMethodChannelCommandService : ICommandService<DeletePaymentMethodChannelCommand>
{
	private readonly IPaymentRepository _paymentRepository;

	public DeletePaymentMethodChannelCommandService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask ExecuteAsync(DeletePaymentMethodChannelCommand command, CancellationToken cancellationToken = default)
		=> _paymentRepository.DeletePaymentMethodChannelAsync(command.PaymentMethodChannelId, cancellationToken);
}
