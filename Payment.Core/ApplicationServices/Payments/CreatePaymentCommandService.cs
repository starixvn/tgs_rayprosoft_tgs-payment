using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class CreatePaymentCommandService : ICommandService<CreatePaymentCommand>
{
	private readonly IPaymentRepository _paymentRepository;

	public CreatePaymentCommandService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask ExecuteAsync(
		CreatePaymentCommand command,
		CancellationToken cancellationToken = default)
		=> _paymentRepository.CreatePaymentAsync(new PaymentCreateInfo(
			command.Code,
			command.Description,
			command.ExtendedData));
}
