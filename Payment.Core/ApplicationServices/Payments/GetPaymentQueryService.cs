using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class GetPaymentQueryService(IQueryService<GetAllPaymentsQuery, PaymentInfo> queryService) : IGetService<GetPaymentQuery, PaymentInfo>
{
	public ValueTask<PaymentInfo> GetAsync(GetPaymentQuery query, CancellationToken cancellationToken = default)
		=> queryService.QueryAsync(new GetAllPaymentsQuery(), cancellationToken)
			.Where(p => p.Id == query.PaymentId)
			.SingleAsync(cancellationToken: cancellationToken);
}
