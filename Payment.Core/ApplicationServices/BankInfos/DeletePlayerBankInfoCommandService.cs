using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class DeletePlayerBankInfoCommandService : ICommandService<DeletePlayerBankInfoCommand>
{
	private readonly IPaymentClient _paymentClient;
	private readonly IPaymentRepository _paymentRepository;

	public DeletePlayerBankInfoCommandService(IPaymentRepository paymentRepository, IPaymentClient paymentClient)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
	}

	public async ValueTask ExecuteAsync(DeletePlayerBankInfoCommand command, CancellationToken cancellationToken = default)
	{
		var bankInfo = await _paymentRepository
			.GetBankInfoByIdAsync(command.BankInfoId, cancellationToken)
			.ConfigureAwait(false);

		if (bankInfo is null)
			return;

		await _paymentRepository
			.DeleteBankInfoAsync(command.PlayerCode, command.BankInfoId, cancellationToken)
			.ConfigureAwait(false);

		await _paymentClient
			.EmitPlayerBankInfoDeletedEventAsync(bankInfo.BrandId, bankInfo.Id, cancellationToken)
			.ConfigureAwait(false);
	}
}
