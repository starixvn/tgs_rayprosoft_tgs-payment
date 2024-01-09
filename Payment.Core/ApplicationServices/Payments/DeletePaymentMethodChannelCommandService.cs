using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class DeletePaymentMethodChannelCommandService(
	IPaymentRepository paymentRepository,
	IPaymentClient paymentClient)
	: ICommandService<DeletePaymentMethodChannelCommand>
{
	public async ValueTask ExecuteAsync(DeletePaymentMethodChannelCommand command, CancellationToken cancellationToken = default)
	{
		await paymentRepository
			.DeletePaymentMethodChannelAsync(command.PaymentMethodChannelId, cancellationToken)
			.ConfigureAwait(false);

		await paymentClient
			.EmitPaymentMethodChannelChangedEventAsync(command.PaymentMethodChannelId, cancellationToken)
			.ConfigureAwait(false);
	}
}
