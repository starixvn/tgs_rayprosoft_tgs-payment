using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class CreatePaymentMethodChannelCommandService(
	IPaymentRepository paymentRepository,
	IPaymentClient paymentClient)
	: IGetService<CreatePaymentMethodChannelCommand, Guid>
{
	public async ValueTask<Guid> GetAsync(CreatePaymentMethodChannelCommand command, CancellationToken cancellationToken = default)
	{
		var id = await paymentRepository.CreatePaymentMethodChannelAsync(
			command.PaymentId,
			command.MethodId,
			command.ChannelId,
			cancellationToken).ConfigureAwait(false);

		await paymentClient
			.EmitPaymentMethodChannelChangedEventAsync(id, cancellationToken)
			.ConfigureAwait(false);

		return id;
	}
}
