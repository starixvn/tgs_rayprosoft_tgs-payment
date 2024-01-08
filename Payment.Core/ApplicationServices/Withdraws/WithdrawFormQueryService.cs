using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class WithdrawFormQueryService(IPaymentRepository paymentRepository)
	: IQueryService<WithdrawFormQuery, WithdrawFormInfo>
	, IGetService<WithdrawFormQuery, int>
{
	public ValueTask<int> GetAsync(WithdrawFormQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.CountWithdrawFormsAsync(
			new WithdrawFormQueryConditions(
				query.BrandId,
				query.UserId,
				query.UserName,
				query.UserType,
				query.ChannelName,
				query.ThirdPartyOrderNo,
				query.OrderNo,
				query.FormId,
				query.PaymentId,
				query.PaymentMethodId,
				query.Status,
				query.BrandPaymentIds,
				query.CreateOnRange,
				query.UpdateOnRange,
				query.ReviewOnRange,
				query.PaidOnRange), cancellationToken);

	public IAsyncEnumerable<WithdrawFormInfo> QueryAsync(WithdrawFormQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.QueryWithdrawFormsAsync(
			new WithdrawFormQueryConditions(
				query.BrandId,
				query.UserId,
				query.UserName,
				query.UserType,
				query.ChannelName,
				query.ThirdPartyOrderNo,
				query.OrderNo,
				query.FormId,
				query.PaymentId,
				query.PaymentMethodId,
				query.Status,
				query.BrandPaymentIds,
				query.CreateOnRange,
				query.UpdateOnRange,
				query.ReviewOnRange,
				query.PaidOnRange),
			query.Paging,
			cancellationToken);
}
