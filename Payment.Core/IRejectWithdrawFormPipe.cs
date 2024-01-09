using System.Threading;
using System.Threading.Tasks;

namespace Tgs.Payment;

public interface IRejectWithdrawFormPipe
{
	public int Order => 1;

	public ValueTask AfterRejectWithdrawFormAsync(
		WithdrawFormSnapshot formData,
		string? reviewUserId,
		string? reviewUserName,
		string? remark,
		CancellationToken cancellationToken = default)
		=> ValueTask.CompletedTask;

	public ValueTask BeforeRejectWithdrawFormAsync(
		WithdrawFormSnapshot formData,
		string? reviewUserId,
		string? reviewUserName,
		string? remark,
		CancellationToken cancellationToken = default)
		=> ValueTask.CompletedTask;
}
