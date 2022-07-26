using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tgs.Payment;

public interface IPaymentRateLimitRepository
{
	ValueTask<Guid> LockPaymentAsync(Guid formId, string paymentCode, string playerCode, CancellationToken cancellationToken = default);

	ValueTask UnlockPaymentAsync(string paymentCode, string playerCode, CancellationToken cancellationToken = default);
}
