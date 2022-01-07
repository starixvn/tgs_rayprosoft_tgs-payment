using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class GetDepositFormDetailByFormIdQueryService : IGetService<GetDepositFormDetailByFormIdQuery, DepositFormInfo?>
{
	private readonly IPaymentRepository _paymentRepository;

	public GetDepositFormDetailByFormIdQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask<DepositFormInfo?> GetAsync(GetDepositFormDetailByFormIdQuery query, CancellationToken cancellationToken = default)
		=> _paymentRepository.GetDepositFormByIdAsync(query.FormId);
}
