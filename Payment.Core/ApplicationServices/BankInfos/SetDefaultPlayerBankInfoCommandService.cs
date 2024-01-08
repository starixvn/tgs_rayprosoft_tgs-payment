using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class SetDefaultPlayerBankInfoCommandService(
	IPaymentRepository paymentRepository)
	: ICommandService<SetDefaultPlayerBankInfoCommand>
{
	public ValueTask ExecuteAsync(
		SetDefaultPlayerBankInfoCommand command,
		CancellationToken cancellationToken = default)
		=> paymentRepository.SetDefaultPlayerBankInfoAsync(
			command.PlayerCode,
			command.BankInfoId,
			command.ModifyUser,
			cancellationToken);
}
