using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class BrandPaymentDesignatedUserQueryService : IQueryService<BrandPaymentDesignatedUserQuery, BrandPaymentPlayerInfo>
{
	private readonly IPaymentRepository _paymentRepository;

	public BrandPaymentDesignatedUserQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public IAsyncEnumerable<BrandPaymentPlayerInfo> QueryAsync(BrandPaymentDesignatedUserQuery query, CancellationToken cancellationToken = default)
		=> _paymentRepository.GetBrandPaymentDesignatedUsersAsync(query.Id, cancellationToken);
}
