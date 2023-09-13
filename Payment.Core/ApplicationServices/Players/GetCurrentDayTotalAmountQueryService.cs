using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Players;

internal class GetCurrentDayTotalAmountQueryService : IQueryService<GetCurrentDayTotalAmountQuery, TopTenDepositWithdrawInfo>
{
	private readonly IPaymentRepository _paymentRepository;

	public GetCurrentDayTotalAmountQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public IAsyncEnumerable<TopTenDepositWithdrawInfo> QueryAsync(GetCurrentDayTotalAmountQuery query, CancellationToken cancellationToken = default)
		=> _paymentRepository.GetCurrentDayDepositWithdrawTotalAmountAsync(query.PlayerCodes, cancellationToken);
}
