using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class SaveWithdrawThirdPartyOrderNoCommandService : ICommandService<SaveWithdrawThirdPartyOrderNoCommand>
{
	private readonly IPaymentRepository _paymentRepository;

	public SaveWithdrawThirdPartyOrderNoCommandService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public async ValueTask ExecuteAsync(SaveWithdrawThirdPartyOrderNoCommand command, CancellationToken cancellationToken = default)
	{
		var formData = await _paymentRepository
			.GetWithdrawFormByIdAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		if (formData == null)
			return;

		await _paymentRepository
			.SubmitWithdrawFormStatusAsync(
				command.FormId,
				formData.Status,
				null,
				null,
				command.ThirdPartyOrderNo,
				null,
				null, 
				cancellationToken)
			.ConfigureAwait(false);
	}
}
