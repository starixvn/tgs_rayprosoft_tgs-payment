using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tgs.Payment;

public interface IApproveWithdrawFormPipe
{
	public int Order => 1;

	public ValueTask AfterApproveWithdrawFormAsync(
		Guid brandPaymentId,
		WithdrawFormSnapshot formData,
		string? reviewUserId,
		string? reviewUserName,
		string? remark,
		CancellationToken cancellationToken = default)
		=> ValueTask.CompletedTask;

	public ValueTask BeforeApproveWithdrawFormAsync(
		Guid brandPaymentId,
		WithdrawFormSnapshot formData,
		string? reviewUserId,
		string? reviewUserName,
		string? remark,
		CancellationToken cancellationToken = default)
		=> ValueTask.CompletedTask;
}
