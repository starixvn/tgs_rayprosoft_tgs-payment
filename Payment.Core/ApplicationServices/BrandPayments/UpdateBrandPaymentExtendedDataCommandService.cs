using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class UpdateBrandPaymentExtendedDataCommandService(
	IPaymentRepository paymentRepository)
	: ICommandService<UpdateBrandPaymentExtendedDataCommand>
{
	public ValueTask ExecuteAsync(UpdateBrandPaymentExtendedDataCommand command, CancellationToken cancellationToken = default)
		=> paymentRepository.UpdateBrandPaymentExtendedDataAsync(command.BrandPaymentId, command.ExtendedData, cancellationToken);
}
