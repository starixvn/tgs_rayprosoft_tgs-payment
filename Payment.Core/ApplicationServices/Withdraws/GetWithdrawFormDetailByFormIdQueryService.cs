using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class GetWithdrawFormDetailByFormIdQueryService : IGetService<GetWithdrawFormDetailByFormIdQuery, WithdrawFormInfo?>
{
	private readonly IPaymentRepository _paymentRepository;

	public GetWithdrawFormDetailByFormIdQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask<WithdrawFormInfo?> GetAsync(
		GetWithdrawFormDetailByFormIdQuery query,
		CancellationToken cancellationToken = default)
		=> _paymentRepository.GetWithdrawFormByIdAsync(query.FormId);
}
