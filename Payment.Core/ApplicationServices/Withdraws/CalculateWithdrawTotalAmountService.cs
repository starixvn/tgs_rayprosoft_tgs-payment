using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class CalculateWithdrawTotalAmountService
	: IGetService<WithdrawFormQuery, decimal>
{
	private readonly IPaymentRepository _paymentRepository;

	public CalculateWithdrawTotalAmountService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask<decimal> GetAsync(WithdrawFormQuery query, CancellationToken cancellationToken = default)
		=> _paymentRepository.CalculateWithdrawTotalAmountAsync(
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
}
