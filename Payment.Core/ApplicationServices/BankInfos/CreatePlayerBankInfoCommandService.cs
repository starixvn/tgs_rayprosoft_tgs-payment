using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class CreatePlayerBankInfoCommandService : ICommandService<CreatePlayerBankInfoCommand>
{
	private readonly IPaymentRepository _paymentRepository;

	public CreatePlayerBankInfoCommandService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public async ValueTask ExecuteAsync(CreatePlayerBankInfoCommand command, CancellationToken cancellationToken = default)
	{
		var bankInfo = new PlayerBankInfo(
			Guid.NewGuid(),
			command.PlayerCode,
			command.BrandId,
			command.PaymentChannelId,
			command.BankType,
			command.Detail,
			command.IsDefault);

		await _paymentRepository
			.CreateBankInfoAsync(bankInfo, command.ModifyUser, cancellationToken)
			.ConfigureAwait(false);

		command.TaskSource.SetResult(bankInfo.Id);
	}
}
