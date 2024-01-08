using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class CreatePaymentChannelCommandService(
	IPaymentRepository paymentRepository,
	IPaymentClient paymentClient)
	: ICommandService<CreatePaymentChannelCommand>
{
	public async ValueTask ExecuteAsync(
		CreatePaymentChannelCommand command,
		CancellationToken cancellationToken = default)
	{
		await paymentRepository
			.CreatePaymentChannelAsync(command.ChannelCode, cancellationToken)
			.ConfigureAwait(false);

		await paymentClient
			.EmitPaymentChannelCreatedEventAsync(command.ChannelCode, cancellationToken)
			.ConfigureAwait(false);
	}
}
