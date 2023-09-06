using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class CreateWithdrawFormCommandService : IGetService<CreateWithdrawFormCommand, Guid>
{
	private readonly IWithdrawService _withdrawService;

	public CreateWithdrawFormCommandService(IWithdrawService withdrawService)
	{
		_withdrawService = withdrawService ?? throw new ArgumentNullException(nameof(withdrawService));
	}

	public ValueTask<Guid> GetAsync(CreateWithdrawFormCommand command, CancellationToken cancellationToken = default)
		=> _withdrawService.CreateWithdrawFormAsync(
			command.User,
			command.BrandId,
			command.BankInfoId,
			command.Amount,
			command.Note,
			cancellationToken);
}
