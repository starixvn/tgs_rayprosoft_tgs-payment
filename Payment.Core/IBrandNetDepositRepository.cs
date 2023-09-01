using System;
using System.Threading.Tasks;

namespace Tgs.Payment;

public interface IBrandNetDepositRepository
{
	ValueTask CountBrandNetDepositAsync(Guid brandId, double amount);

	ValueTask<decimal> GetBrandNetDepositAsync(Guid brandId);
}
