using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Tgs.Payment.Clients;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws
{
	internal class CreateWithdrawFormCommandService : ICommandService<CreateWithdrawFormCommand>
	{
		private readonly JsonSerializerOptions? _jsonSerializerOptions;
		private readonly IPaymentClient _paymentClient;
		private readonly IPaymentRepository _paymentRepository;
		private readonly ITakeNumberStore _takeNumberStore;

		public CreateWithdrawFormCommandService(
			IPaymentRepository paymentRepository,
			IPaymentClient paymentClient,
			IOptions<PaymentOptions> paymentOptionsAccessor,
			ITakeNumberStore takeNumberStore)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
			_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
			_takeNumberStore = takeNumberStore ?? throw new ArgumentNullException(nameof(takeNumberStore));
			_jsonSerializerOptions = paymentOptionsAccessor.Value.JsonSerializerOptions;
		}

		public async ValueTask ExecuteAsync(CreateWithdrawFormCommand command, CancellationToken cancellationToken = default)
		{
			var brandInfo = await _paymentRepository.GetBrandInfoByIdAsync(command.BrandId);

			var orderNo = await _takeNumberStore.GetWithdrawFormOrderNoAsync(brandInfo.Code);

			var createFormId = command.UserType switch
			{
				UserType.Player => await CreatePlayerWithdrawFormAsync(
					command,
					orderNo,
					cancellationToken).ConfigureAwait(false),
				UserType.Agent => await CreateAgentWithdrawFormAsync(
					command,
					orderNo,
					cancellationToken).ConfigureAwait(false),
				_ => throw new InvalidOperationException($"Can't handle user type: {command.UserType}")
			};

			command.ReturnFormIdSource.SetResult(createFormId);
		}

		private async ValueTask<Guid> CreateAgentWithdrawFormAsync(
			CreateWithdrawFormCommand command,
			string orderNo,
			CancellationToken cancellationToken = default)
		{
			var createFormId = Guid.NewGuid();
			var agentId = Guid.Parse(command.UserId!);

			var agentBankInfo = await _paymentRepository
					.GetAgentBankInfoByIdAsync(command.BankInfoId)
					.ConfigureAwait(false);

			if (agentBankInfo.AgentId != agentId)
				throw new InvalidOperationException("Can't find bank info.");

			var bankInfoData = new WithdrawBankInfo<JsonDocument>(
				agentBankInfo.Id,
				agentBankInfo.PaymentChannelId,
				agentBankInfo.BankType,
				JsonDocument.Parse(agentBankInfo.Detail));

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
						BankInfo: JsonSerializer.Serialize(bankInfoData, _jsonSerializerOptions),
						OrderNo: orderNo))
				.ConfigureAwait(false);

			await _paymentClient
				.EmitWithdrawFormCreatedEventAsync(createFormId, command.BrandId)
				.ConfigureAwait(false);

			return createFormId;
		}

		private async ValueTask<Guid> CreatePlayerWithdrawFormAsync(
			CreateWithdrawFormCommand command,
			string orderNo,
			CancellationToken cancellationToken = default)
		{
			var createFormId = Guid.NewGuid();

			var playerBankInfo = await _paymentRepository
					.GetBankInfoByIdAsync(command.BankInfoId)
					.ConfigureAwait(false);

			if (playerBankInfo.PlayerCode != command.UserId)
				throw new InvalidOperationException("Can't find bank info.");

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
						BankInfo: JsonSerializer.Serialize(bankInfoData, _jsonSerializerOptions),
						OrderNo: orderNo))
				.ConfigureAwait(false);

			await _paymentClient
				.EmitWithdrawFormCreatedEventAsync(createFormId, command.BrandId)
				.ConfigureAwait(false);

			return createFormId;
		}
	}
}
