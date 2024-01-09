using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class GetCurrentDayTopTenDepositPlayersQueryService(IPaymentRepository paymentRepository)
	: IQueryService<GetCurrentDayTopTenDepositPlayersQuery, TopTenDepositWithdrawInfo>
{
	public IAsyncEnumerable<TopTenDepositWithdrawInfo> QueryAsync(GetCurrentDayTopTenDepositPlayersQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.GetCurrentDayTopTenDepositPlayersAsync(query.BrandId, cancellationToken);
}
