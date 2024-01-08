using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Settings;

internal class SetPlayerBankInfoDuplicateCheckIsEnabledCommandService(IPaymentRepository paymentRepository)
	: ICommandService<SetPlayerBankInfoDuplicateCheckIsEnabledCommand>
{
	public ValueTask ExecuteAsync(SetPlayerBankInfoDuplicateCheckIsEnabledCommand command, CancellationToken cancellationToken = default)
		=> paymentRepository.SetBrandCheckPlayerBankInfoDuplicateEnabledAsync(
			command.BrandId,
			command.IsEnabled,
			cancellationToken);
}
