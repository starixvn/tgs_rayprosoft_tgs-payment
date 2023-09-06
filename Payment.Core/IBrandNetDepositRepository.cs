using System;
using System.Threading.Tasks;

namespace Tgs.Payment;

public interface IBrandNetDepositRepository
{
	ValueTask CountBrandCurrentDayNetDepositAsync(Guid brandId, double amount);

	ValueTask<decimal> GetBrandCurrentDayNetDepositAsync(Guid brandId);

	ValueTask<decimal> GetBrandYesterDayNetDepositAsync(Guid brandId);
}
