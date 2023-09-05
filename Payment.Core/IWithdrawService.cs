using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tgs.Payment;
public interface IWithdrawService
{
	ValueTask<Guid> CreateWithdrawFormAsync(
		WithdrawUser user,
		Guid brandId,
		Guid bankInfoId,
		decimal amount,
		string? note,
		CancellationToken cancellationToken = default);
}