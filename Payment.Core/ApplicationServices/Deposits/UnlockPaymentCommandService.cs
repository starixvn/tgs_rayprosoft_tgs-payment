using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class UnlockPaymentCommandService : ICommandService<UnlockPaymentCommand>
{
	private readonly IPaymentRateLimitRepository _paymentRateLimitRepository;

	public UnlockPaymentCommandService(IPaymentRateLimitRepository paymentRateLimitRepository)
	{
		_paymentRateLimitRepository = paymentRateLimitRepository ?? throw new ArgumentNullException(nameof(paymentRateLimitRepository));
	}

	public ValueTask ExecuteAsync(UnlockPaymentCommand command, CancellationToken cancellationToken = default)
		=> _paymentRateLimitRepository.UnlockPaymentAsync(
			command.PaymentCode,
			command.PlayerCode,
			cancellationToken);
}
