using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class CreatePlayerBankInfoCommandService : IGetService<CreatePlayerBankInfoCommand, Guid>
{
	private readonly IPaymentClient _paymentClient;
	private readonly IPaymentRepository _paymentRepository;

	public CreatePlayerBankInfoCommandService(IPaymentRepository paymentRepository, IPaymentClient paymentClient)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
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

		await _paymentClient
			.EmitPlayerBankInfoAddedEventAsync(command.BrandId, bankInfo.Id, cancellationToken)
			.ConfigureAwait(false);

		return bankInfo.Id;
	}
}
