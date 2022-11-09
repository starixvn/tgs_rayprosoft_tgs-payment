using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class UpdateBrandPaymentExtendedDataCommandService : ICommandService<UpdateBrandPaymentExtendedDataCommand>
{
	private readonly IPaymentRepository _paymentRepository;

	public UpdateBrandPaymentExtendedDataCommandService(
		IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask ExecuteAsync(UpdateBrandPaymentExtendedDataCommand command, CancellationToken cancellationToken = default)
		=> _paymentRepository.UpdateBrandPaymentExtendedDataAsync(command.BrandPaymentId, command.ExtendedData, cancellationToken);
}
