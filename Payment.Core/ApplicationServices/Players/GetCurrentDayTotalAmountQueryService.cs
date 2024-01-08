using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Players;

internal class GetCurrentDayTotalAmountQueryService(IPaymentRepository paymentRepository)
	: IQueryService<GetCurrentDayTotalAmountQuery, TopTenDepositWithdrawInfo>
{
	public IAsyncEnumerable<TopTenDepositWithdrawInfo> QueryAsync(GetCurrentDayTotalAmountQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.GetCurrentDayDepositWithdrawTotalAmountAsync(query.PlayerCodes, cancellationToken);
}
