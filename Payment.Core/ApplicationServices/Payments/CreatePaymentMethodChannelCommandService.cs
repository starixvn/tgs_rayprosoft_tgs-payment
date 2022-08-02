using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class CreatePaymentMethodChannelCommandService : IGetService<CreatePaymentMethodChannelCommand, Guid>
{
	private readonly IPaymentRepository _paymentRepository;

	public CreatePaymentMethodChannelCommandService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask<Guid> GetAsync(CreatePaymentMethodChannelCommand command, CancellationToken cancellationToken = default)
		=> _paymentRepository.CreatePaymentMethodChannelAsync(
			command.PaymentId,
			command.MethodId,
			command.ChannelId,
			cancellationToken);
}
