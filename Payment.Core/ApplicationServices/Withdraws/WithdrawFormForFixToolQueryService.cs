using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class WithdrawFormForFixToolQueryService : IQueryService<WithdrawFormForFixToolQuery, WithdrawFormInfo>
{
	private readonly IPaymentRepository _paymentRepository;

	public WithdrawFormForFixToolQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public IAsyncEnumerable<WithdrawFormInfo> QueryAsync(WithdrawFormForFixToolQuery query, CancellationToken cancellationToken = default)
		=> _paymentRepository.QueryWithdrawFormsAsync(
			WithdrawFormQueryConditions.Default with
			{
				BrandId = query.BrandId,
				UserId = query.UserId,
				UserType = query.UserType
			},
			false,
			cancellationToken);
}
