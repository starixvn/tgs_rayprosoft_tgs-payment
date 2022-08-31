using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class CreatePlayerBankInfoCommandService : IGetService<CreatePlayerBankInfoCommand, Guid>
{
	private readonly IPaymentRepository _paymentRepository;

	public CreatePlayerBankInfoCommandService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public async ValueTask<Guid> GetAsync(CreatePlayerBankInfoCommand command, CancellationToken cancellationToken = default)
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

		return bankInfo.Id;
	}
}
