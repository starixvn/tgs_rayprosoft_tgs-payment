using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class CreateWithdrawFormCommandService(IWithdrawService withdrawService) : IGetService<CreateWithdrawFormCommand, Guid>
{
	public ValueTask<Guid> GetAsync(CreateWithdrawFormCommand command, CancellationToken cancellationToken = default)
		=> withdrawService.CreateWithdrawFormAsync(
			command.User,
			command.BrandId,
			command.BankInfoId,
			command.Amount,
			command.Note,
			cancellationToken);
}
