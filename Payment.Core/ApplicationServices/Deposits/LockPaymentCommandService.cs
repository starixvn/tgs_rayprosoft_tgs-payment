using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class LockPaymentCommandService(
	IPaymentRateLimitRepository paymentRateLimitRepository,
	IGetService<GetDepositFormDetailByFormIdQuery, DepositFormInfo?> getFormService) : IGetService<LockPaymentCommand, Guid>
{
	public async ValueTask<Guid> GetAsync(LockPaymentCommand command, CancellationToken cancellationToken = default)
	{
		var lockedFormId = await paymentRateLimitRepository.LockPaymentAsync(
			command.FormId,
			command.PaymentCode,
			command.PlayerCode,
			cancellationToken).ConfigureAwait(false);

		if (lockedFormId != command.FormId)
		{
			var query = new GetDepositFormDetailByFormIdQuery(lockedFormId);

			var formInfo = await getFormService
				.GetAsync(query, cancellationToken)
				.ConfigureAwait(false);

			if (formInfo is null)
				return lockedFormId;

			if (formInfo.Status is PaymentFlowState.Approved or PaymentFlowState.Reject)
			{
				await paymentRateLimitRepository.UnlockPaymentAsync(
					command.PaymentCode,
					command.PlayerCode,
					cancellationToken).ConfigureAwait(false);

				return await paymentRateLimitRepository.LockPaymentAsync(
					command.FormId,
					command.PaymentCode,
					command.PlayerCode,
					cancellationToken).ConfigureAwait(false);
			}
		}

		return lockedFormId;
	}
}
