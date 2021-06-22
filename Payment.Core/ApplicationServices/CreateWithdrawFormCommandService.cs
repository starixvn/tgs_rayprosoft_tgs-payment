using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices
{
	internal class CreateWithdrawFormCommandService : ICommandService<CreateWithdrawFormCommand>
	{
		private readonly IPaymentRepository _paymentRepository;
		private readonly IPaymentClient _paymentClient;

		public CreateWithdrawFormCommandService(
			IPaymentRepository paymentRepository,
			IPaymentClient paymentClient)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
			_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
		}

		public async ValueTask ExecuteAsync(CreateWithdrawFormCommand command, CancellationToken cancellationToken = default)
		{
			var createFormId = Guid.NewGuid();

			if (command.UserType == UserType.Player)
			{
				var playerBankInfo = await _paymentRepository
					.GetBankInfoByIdAsync(command.BankInfoId)
					.ConfigureAwait(false);

				var bankInfoData = new WithdrawBankInfo<JsonDocument>(
					command.BankInfoId,
					playerBankInfo.PaymentChannelId,
					playerBankInfo.BankType,
					JsonDocument.Parse(playerBankInfo.Detail));

				await _paymentRepository.CreateWithdrawFormAsync(
					createFormId,
					new WithdrawFormCreateInfo(
						command.UserId,
						command.UserName,
						command.UserType,
						command.BrandId,
						command.Amount,
						PaymentFlowState.Init,
						null,
						command.BankInfoId)).ConfigureAwait(false);

				await _paymentClient
					.EmitWithdrawFormCreatedEventAsync(createFormId, command.BrandId)
					.ConfigureAwait(false);

				command.ReturnFormIdSource.SetResult(createFormId);
			}
		}
	}
}
