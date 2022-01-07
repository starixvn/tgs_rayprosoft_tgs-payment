using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class CreatePaymentMethodCommandService : ICommandService<CreatePaymentMethodCommand>
{
	private readonly IPaymentRepository _paymentRepository;

	public CreatePaymentMethodCommandService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask ExecuteAsync(
		CreatePaymentMethodCommand command,
		CancellationToken cancellationToken = default)
		=> _paymentRepository.CreatePaymentMethodAsync(command.Code, command.BankType);
}
