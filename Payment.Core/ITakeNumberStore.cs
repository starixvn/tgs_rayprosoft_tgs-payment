using System.Threading.Tasks;

namespace Tgs.Payment;

public interface ITakeNumberStore
{
	ValueTask<string> GetDepositFormOrderNoAsync(string BrandCode);

	ValueTask<string> GetWithdrawFormOrderNoAsync(string BrandCode);
}
