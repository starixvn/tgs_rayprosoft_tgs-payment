using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class DeletePlayerBankInfoCommandService(IPaymentRepository paymentRepository, IPaymentClient paymentClient)
	: ICommandService<DeletePlayerBankInfoCommand>
{
	public async ValueTask ExecuteAsync(DeletePlayerBankInfoCommand command, CancellationToken cancellationToken = default)
	{
		var bankInfo = await paymentRepository
			.GetBankInfoByIdAsync(command.BankInfoId, cancellationToken)
			.ConfigureAwait(false);

		if (bankInfo is null)
			return;

		await paymentRepository
			.DeleteBankInfoAsync(command.PlayerCode, command.BankInfoId, cancellationToken)
			.ConfigureAwait(false);

		await paymentClient
			.EmitPlayerBankInfoDeletedEventAsync(bankInfo.BrandId, bankInfo.Id, cancellationToken)
			.ConfigureAwait(false);
	}
}
