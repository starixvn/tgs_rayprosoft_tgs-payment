using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class GetBrandPaymentFeeQueryService : IGetService<GetBrandPaymentFeeQuery, BrandPaymentFeeInfo>
{
	private readonly IPaymentRepository _paymentRepository;

	public GetBrandPaymentFeeQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask<BrandPaymentFeeInfo> GetAsync(GetBrandPaymentFeeQuery query, CancellationToken cancellationToken = default)
	{
		return _paymentRepository.GetBrandPaymentFeeSettingAsync(query.Id, cancellationToken);
	}
}
