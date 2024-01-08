using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class CalculateDepositTotalAmountService(IPaymentRepository paymentRepository)
	: IGetService<DepositFormQuery, decimal>
{
	public ValueTask<decimal> GetAsync(DepositFormQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.CalculateDepositTotalAmountAsync(new DepositFormQueryConditions(
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
}
