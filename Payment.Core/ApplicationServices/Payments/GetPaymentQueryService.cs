using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class GetPaymentQueryService : IGetService<GetPaymentQuery, PaymentInfo>
{
	private readonly IQueryService<GetAllPaymentsQuery, PaymentInfo> _queryService;

	public GetPaymentQueryService(IQueryService<GetAllPaymentsQuery, PaymentInfo> queryService)
	{
		_queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
	}

	public ValueTask<PaymentInfo> GetAsync(GetPaymentQuery query, CancellationToken cancellationToken = default)
		=> _queryService.QueryAsync(new GetAllPaymentsQuery(), cancellationToken)
			.Where(p => p.Id == query.PaymentId)
			.SingleAsync(cancellationToken: cancellationToken);
}
