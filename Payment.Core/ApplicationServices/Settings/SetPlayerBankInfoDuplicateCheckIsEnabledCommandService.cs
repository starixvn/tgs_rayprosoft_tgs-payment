using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Settings;

internal class SetPlayerBankInfoDuplicateCheckIsEnabledCommandService : ICommandService<SetPlayerBankInfoDuplicateCheckIsEnabledCommand>
{
	private readonly IPaymentRepository _paymentRepository;

	public SetPlayerBankInfoDuplicateCheckIsEnabledCommandService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask ExecuteAsync(SetPlayerBankInfoDuplicateCheckIsEnabledCommand command, CancellationToken cancellationToken = default)
		=> _paymentRepository.SetBrandCheckPlayerBankInfoDuplicateEnabledAsync(
			command.BrandId,
			command.IsEnabled,
			cancellationToken);
}
