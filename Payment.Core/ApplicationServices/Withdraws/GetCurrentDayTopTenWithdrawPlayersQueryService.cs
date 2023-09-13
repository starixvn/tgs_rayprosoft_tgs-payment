using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class GetCurrentDayTopTenWithdrawPlayersQueryService : IQueryService<GetCurrentDayTopTenWithdrawPlayersQuery, TopTenDepositWithdrawInfo>
{
	private readonly IPaymentRepository _paymentRepository;

	public GetCurrentDayTopTenWithdrawPlayersQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public IAsyncEnumerable<TopTenDepositWithdrawInfo> QueryAsync(GetCurrentDayTopTenWithdrawPlayersQuery query, CancellationToken cancellationToken = default)
		=> _paymentRepository.GetCurrentDayTopTenWithdrawPlayersAsync(query.BrandId, cancellationToken);
}
