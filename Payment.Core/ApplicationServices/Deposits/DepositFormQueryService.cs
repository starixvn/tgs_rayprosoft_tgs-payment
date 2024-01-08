using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class DepositFormQueryService(IPaymentRepository paymentRepository)
	: IQueryService<DepositFormQuery, DepositFormInfo>
	, IGetService<DepositFormQuery, int>
{
	public ValueTask<int> GetAsync(DepositFormQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.CountDepositFormsAsync(new DepositFormQueryConditions(
			query.BrandId,
			query.UserId,
			query.UserName,
			query.ChannelName,
			query.ThirdPartyOrderNo,
			query.OrderNo,
			query.FormId,
			query.PaymentId,
			query.PaymentMethodId,
			query.Status,
			query.CreateOnRange,
			query.UpdateOnRange), cancellationToken);

	public IAsyncEnumerable<DepositFormInfo> QueryAsync(DepositFormQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.QueryDepositFormsAsync(
			new DepositFormQueryConditions(
				query.BrandId,
				query.UserId,
				query.UserName,
				query.ChannelName,
				query.ThirdPartyOrderNo,
				query.OrderNo,
				query.FormId,
				query.PaymentId,
				query.PaymentMethodId,
				query.Status,
				query.CreateOnRange,
				query.UpdateOnRange),
			query.Paging,
			cancellationToken);
}
