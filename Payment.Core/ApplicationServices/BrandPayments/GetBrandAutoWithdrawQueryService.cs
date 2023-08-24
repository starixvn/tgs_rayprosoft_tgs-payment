using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class GetBrandAutoWithdrawQueryService : IGetService<GetBrandAutoWithdrawQuery, AutoWithdrawInfo?>
{
	private readonly IPaymentRepository _paymentRepository;

	public GetBrandAutoWithdrawQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask<AutoWithdrawInfo?> GetAsync(GetBrandAutoWithdrawQuery query, CancellationToken cancellationToken = default)
		=> _paymentRepository.GetBrandAutoWithdrawSettingAsync(query.BrandId, cancellationToken);
}
