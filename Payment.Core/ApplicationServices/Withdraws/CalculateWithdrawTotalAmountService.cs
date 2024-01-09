using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class CalculateWithdrawTotalAmountService(IPaymentRepository paymentRepository)
	: IGetService<WithdrawFormQuery, decimal>
{
	public ValueTask<decimal> GetAsync(WithdrawFormQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.CalculateWithdrawTotalAmountAsync(
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
			cancellationToken);
}
