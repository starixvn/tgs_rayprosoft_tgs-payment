using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class WithdrawFormForFixToolQueryService(IPaymentRepository paymentRepository) : IQueryService<WithdrawFormForFixToolQuery, WithdrawFormInfo>
{
	public IAsyncEnumerable<WithdrawFormInfo> QueryAsync(WithdrawFormForFixToolQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.QueryWithdrawFormsAsync(
			WithdrawFormQueryConditions.Default with
			{
				BrandId = query.BrandId,
				UserId = query.UserId,
				UserType = query.UserType
			},
			false,
			cancellationToken);
}
