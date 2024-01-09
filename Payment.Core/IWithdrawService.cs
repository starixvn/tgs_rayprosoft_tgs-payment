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

	ValueTask ApproveWithdrawFormAsync(
		Guid brandPaymentId,
		WithdrawFormSnapshot formData,
		string? reviewUserId,
		string? reviewUserName,
		string? remark,
		CancellationToken cancellationToken = default);

	ValueTask RejectWithdrawFormAsync(
		WithdrawFormSnapshot formData,
		string? reviewUserId,
		string? reviewUserName,
		string? remark,
		CancellationToken cancellationToken = default);
}
