using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class CalculateDepositTotalAmountService
	: IGetService<DepositFormQuery, decimal>
{
	private readonly IPaymentRepository _paymentRepository;

	public CalculateDepositTotalAmountService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask<decimal> GetAsync(DepositFormQuery query, CancellationToken cancellationToken = default)
		=> _paymentRepository.CalculateDepositTotalAmountAsync(new DepositFormQueryConditions(
			query.BrandId,
			query.UserId,
			query.UserName,
			query.ChannelName,
			query.ThirdPartyOrderNo,
			query.OrderNo,
			query.FormId,
			query.PaymentMethodId,
			query.Status,
			query.CreateOnRange,
			query.UpdateOnRange), cancellationToken);
}
