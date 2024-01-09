using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class CreatePaymentMethodCommandService(
	IPaymentRepository paymentRepository,
	IPaymentClient paymentClient)
	: ICommandService<CreatePaymentMethodCommand>
{
	public async ValueTask ExecuteAsync(
		CreatePaymentMethodCommand command,
		CancellationToken cancellationToken = default)
	{
		await paymentRepository
			.CreatePaymentMethodAsync(command.Code, command.BankType, cancellationToken)
			.ConfigureAwait(false);

		await paymentClient
			.EmitPaymentMethodCreatedEventAsync(command.Code, cancellationToken)
			.ConfigureAwait(false);
	}
}
