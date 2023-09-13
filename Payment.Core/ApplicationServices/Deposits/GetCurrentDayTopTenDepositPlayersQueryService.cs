using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class GetCurrentDayTopTenDepositPlayersQueryService : IQueryService<GetCurrentDayTopTenDepositPlayersQuery, TopTenDepositWithdrawInfo>
{
	private readonly IPaymentRepository _paymentRepository;

	public GetCurrentDayTopTenDepositPlayersQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public IAsyncEnumerable<TopTenDepositWithdrawInfo> QueryAsync(GetCurrentDayTopTenDepositPlayersQuery query, CancellationToken cancellationToken = default)
		=> _paymentRepository.GetCurrentDayTopTenDepositPlayersAsync(query.BrandId, cancellationToken);
}
