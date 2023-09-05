using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tgs.Payment;

public interface ICreateWithdrawFormPipe
{
	public int Order => 1;

	public ValueTask AfterCreateWithdrawFormAsync(
		WithdrawUser user,
		Guid brandId,
		Guid bankInfoId,
		decimal amount,
		string? note,
		Guid createFormId,
		CancellationToken cancellationToken = default)
		=> ValueTask.CompletedTask;

	public ValueTask BeforeCreateWithdrawFormAsync(
		WithdrawUser user,
		Guid brandId,
		Guid bankInfoId,
		decimal amount,
		string? note,
		CancellationToken cancellationToken = default)
		=> ValueTask.CompletedTask;
}
