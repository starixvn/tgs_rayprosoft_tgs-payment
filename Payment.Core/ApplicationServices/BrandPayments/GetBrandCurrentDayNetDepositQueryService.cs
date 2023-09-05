using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class GetBrandCurrentDayNetDepositQueryService : IGetService<GetBrandCurrentDayNetDepositQuery, decimal>
{
	private readonly IBrandNetDepositRepository _repository;

	public GetBrandCurrentDayNetDepositQueryService(IBrandNetDepositRepository repository)
	{
		_repository = repository ?? throw new ArgumentNullException(nameof(repository));
	}

	public ValueTask<decimal> GetAsync(GetBrandCurrentDayNetDepositQuery query, CancellationToken cancellationToken = default)
		=> _repository.GetBrandCurrentDayNetDepositAsync(query.BrandId);
}
