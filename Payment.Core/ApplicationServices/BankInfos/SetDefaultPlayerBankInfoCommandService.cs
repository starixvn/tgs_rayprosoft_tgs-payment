using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class SetDefaultPlayerBankInfoCommandService : ICommandService<SetDefaultPlayerBankInfoCommand>
{
	private readonly IPaymentRepository _paymentRepository;

	public SetDefaultPlayerBankInfoCommandService(
		IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask ExecuteAsync(
		SetDefaultPlayerBankInfoCommand command,
		CancellationToken cancellationToken = default)
		=> _paymentRepository
			.SetDefaultPlayerBankInfoAsync(command.PlayerCode, command.BankInfoId, command.ModifyUser, cancellationToken);
}
