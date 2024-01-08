using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class GetBrandCurrentDayNetDepositQueryService(IBrandNetDepositRepository repository) : IGetService<GetBrandCurrentDayNetDepositQuery, decimal>
{
	public ValueTask<decimal> GetAsync(GetBrandCurrentDayNetDepositQuery query, CancellationToken cancellationToken = default)
		=> repository.GetBrandCurrentDayNetDepositAsync(query.BrandId);
}
