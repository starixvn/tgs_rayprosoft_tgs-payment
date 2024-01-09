using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class GetCurrentDayTopTenWithdrawPlayersQueryService(IPaymentRepository paymentRepository)
	: IQueryService<GetCurrentDayTopTenWithdrawPlayersQuery, TopTenDepositWithdrawInfo>
{
	public IAsyncEnumerable<TopTenDepositWithdrawInfo> QueryAsync(GetCurrentDayTopTenWithdrawPlayersQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.GetCurrentDayTopTenWithdrawPlayersAsync(query.BrandId, cancellationToken);
}
