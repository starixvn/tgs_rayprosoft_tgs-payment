using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class UnlockPaymentCommandService(IPaymentRateLimitRepository paymentRateLimitRepository) : ICommandService<UnlockPaymentCommand>
{
	public ValueTask ExecuteAsync(UnlockPaymentCommand command, CancellationToken cancellationToken = default)
		=> paymentRateLimitRepository.UnlockPaymentAsync(
			command.PaymentCode,
			command.PlayerCode,
			cancellationToken);
}
