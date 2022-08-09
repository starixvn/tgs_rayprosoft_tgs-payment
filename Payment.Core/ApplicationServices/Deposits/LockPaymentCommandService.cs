using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class LockPaymentCommandService : IGetService<LockPaymentCommand, Guid>
{
	private readonly IGetService<GetDepositFormDetailByFormIdQuery, DepositFormInfo?> _getFormService;
	private readonly IPaymentRateLimitRepository _paymentRateLimitRepository;

	public LockPaymentCommandService(
		IPaymentRateLimitRepository paymentRateLimitRepository,
		IGetService<GetDepositFormDetailByFormIdQuery, DepositFormInfo?> getFormService)
	{
		_paymentRateLimitRepository = paymentRateLimitRepository ?? throw new ArgumentNullException(nameof(paymentRateLimitRepository));
		_getFormService = getFormService ?? throw new ArgumentNullException(nameof(getFormService));
	}

	public async ValueTask<Guid> GetAsync(LockPaymentCommand command, CancellationToken cancellationToken = default)
	{
		var lockedFormId = await _paymentRateLimitRepository.LockPaymentAsync(
			command.FormId,
			command.PaymentCode,
			command.PlayerCode,
			cancellationToken).ConfigureAwait(false);

		if (lockedFormId != command.FormId)
		{
			var query = new GetDepositFormDetailByFormIdQuery(lockedFormId);

			var formInfo = await _getFormService
				.GetAsync(query, cancellationToken)
				.ConfigureAwait(false);

			if (formInfo is null)
				return lockedFormId;

			if (formInfo.Status is PaymentFlowState.Approved or PaymentFlowState.Reject)
			{
				await _paymentRateLimitRepository.UnlockPaymentAsync(
					command.PaymentCode,
					command.PlayerCode,
					cancellationToken).ConfigureAwait(false);

				return await _paymentRateLimitRepository.LockPaymentAsync(
					command.FormId,
					command.PaymentCode,
					command.PlayerCode,
					cancellationToken).ConfigureAwait(false);
			}
		}

		return lockedFormId;
	}
}
