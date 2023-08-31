using System;
using System.Threading.Tasks;

namespace Tgs.Payment;

public interface ICountRepository
{
	ValueTask CountAmountAsync(Guid brandId, double amount);
}
