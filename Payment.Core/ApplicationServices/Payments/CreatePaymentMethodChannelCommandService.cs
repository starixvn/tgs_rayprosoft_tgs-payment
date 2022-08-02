using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class CreatePaymentMethodChannelCommandService : IGetService<CreatePaymentMethodChannelCommand, Guid>
{
	private readonly IPaymentClient _paymentClient;
	private readonly IPaymentRepository _paymentRepository;

	public CreatePaymentMethodChannelCommandService(IPaymentRepository paymentRepository, IPaymentClient paymentClient)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
	}

	public async ValueTask<Guid> GetAsync(CreatePaymentMethodChannelCommand command, CancellationToken cancellationToken = default)
	{
		var id = await _paymentRepository.CreatePaymentMethodChannelAsync(
			command.PaymentId,
			command.MethodId,
			command.ChannelId,
			cancellationToken).ConfigureAwait(false);

		await _paymentClient
			.EmitPaymentMethodChannelChangedEventAsync(id, cancellationToken)
			.ConfigureAwait(false);

		return id;
	}
}
