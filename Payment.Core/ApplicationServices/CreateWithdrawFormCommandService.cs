using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Tgs.Payment.Clients;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices
{
	internal class CreateWithdrawFormCommandService : ICommandService<CreateWithdrawFormCommand>
	{
		private readonly JsonSerializerOptions? _jsonSerializerOptions;
		private readonly IPaymentClient _paymentClient;
		private readonly IPaymentRepository _paymentRepository;

		public CreateWithdrawFormCommandService(
			IPaymentRepository paymentRepository,
			IPaymentClient paymentClient,
			IOptions<PaymentOptions> paymentOptionsAccessor)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
			_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
			_jsonSerializerOptions = paymentOptionsAccessor.Value.JsonSerializerOptions;
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

				await _paymentRepository
					.CreateWithdrawFormAsync(
						createFormId,
						new WithdrawFormCreateInfo(
							command.UserId,
							command.UserName,
							command.UserType,
							command.BrandId,
							command.Amount,
							PaymentFlowState.Init,
							null,
							command.BankInfoId,
							BankInfo: JsonSerializer.Serialize(bankInfoData, _jsonSerializerOptions)))
					.ConfigureAwait(false);

				await _paymentClient
					.EmitWithdrawFormCreatedEventAsync(createFormId, command.BrandId)
					.ConfigureAwait(false);

				command.ReturnFormIdSource.SetResult(createFormId);
			}
		}
	}
}
