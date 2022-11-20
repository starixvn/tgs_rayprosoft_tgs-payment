using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Settings;

internal class GetPlayerBankInfoDuplicateCheckIsEnabledQueryService : IGetService<GetPlayerBankInfoDuplicateCheckIsEnabledQuery, bool>
{
	private readonly IPaymentRepository _paymentRepository;

	public GetPlayerBankInfoDuplicateCheckIsEnabledQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask<bool> GetAsync(GetPlayerBankInfoDuplicateCheckIsEnabledQuery query, CancellationToken cancellationToken = default)
		=> _paymentRepository.GetBrandCheckPlayerBankInfoDuplicateIsEnabledAsync(query.BrandId, cancellationToken);
}
