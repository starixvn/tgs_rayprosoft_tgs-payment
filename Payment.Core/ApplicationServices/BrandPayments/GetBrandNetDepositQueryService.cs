using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class GetBrandNetDepositQueryService : IGetService<GetBrandNetDepositQuery, decimal>
{
	private readonly IBrandNetDepositRepository _repository;

	public GetBrandNetDepositQueryService(IBrandNetDepositRepository repository)
	{
		_repository = repository ?? throw new ArgumentNullException(nameof(repository));
	}

	public ValueTask<decimal> GetAsync(GetBrandNetDepositQuery query, CancellationToken cancellationToken = default)
		=> _repository.GetBrandNetDepositAsync(query.BrandId);
}
