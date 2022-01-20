using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class SubmitDepositFormThirdPartyOrderNoCommandService : ICommandService<SubmitDepositFormThirdPartyOrderNoCommand>
{
	private readonly IPaymentRepository _paymentRepository;

	public SubmitDepositFormThirdPartyOrderNoCommandService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public async ValueTask ExecuteAsync(SubmitDepositFormThirdPartyOrderNoCommand command, CancellationToken cancellationToken = default)
	{
		var formData = await _paymentRepository
			.GetDepositFormByIdAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		if (formData == null)
			return;

		await _paymentRepository
			.SubmitDepositFormStatusAsync(
				command.FormId,
				formData.Status,
				null,
				null,
				command.ThirdPartyOrderNo,
				null, 
				cancellationToken)
			.ConfigureAwait(false);
	}
}
