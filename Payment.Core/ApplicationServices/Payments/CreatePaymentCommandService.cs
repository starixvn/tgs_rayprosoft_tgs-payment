using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class CreatePaymentCommandService(
	IPaymentRepository paymentRepository,
	IPaymentClient paymentClient)
	: ICommandService<CreatePaymentCommand>
{
	public async ValueTask ExecuteAsync(
		CreatePaymentCommand command,
		CancellationToken cancellationToken = default)
	{
		await paymentRepository.CreatePaymentAsync(
			new PaymentCreateInfo(
				command.Code,
				command.Description,
				command.ExtendedData),
			cancellationToken).ConfigureAwait(false);

		await paymentClient
			.EmitPaymentCreatedEventAsync(command.Code, cancellationToken)
			.ConfigureAwait(false);
	}
}
