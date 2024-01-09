using System.Threading;
using System.Threading.Tasks;

namespace Tgs.Payment;

public interface IPaidWithdrawFormPipe
{
	public int Order => 1;

	public ValueTask AfterPaidWithdrawFormAsync(
		WithdrawFormSnapshot formData,
		string? reviewUserId,
		string? reviewUserName,
		string? remark,
		CancellationToken cancellationToken = default)
		=> ValueTask.CompletedTask;

	public ValueTask BeforePaidWithdrawFormAsync(
		WithdrawFormSnapshot formData,
		string? reviewUserId,
		string? reviewUserName,
		string? remark,
		CancellationToken cancellationToken = default)
		=> ValueTask.CompletedTask;
}
