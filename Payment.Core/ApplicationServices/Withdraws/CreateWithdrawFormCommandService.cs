using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Json.Patch;
using Json.Pointer;
using Tgs.Payment.Clients;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class CreateWithdrawFormCommandService : IGetService<CreateWithdrawFormCommand, Guid>
{
	private readonly IPaymentClient _paymentClient;
	private readonly IPaymentRepository _paymentRepository;
	private readonly ISerialNumberGenerator _serialNumberGenerator;
	private readonly ISystemClock _systemClock;
	private readonly ITakeNumberStore _takeNumberStore;

	public CreateWithdrawFormCommandService(
		IPaymentRepository paymentRepository,
		IPaymentClient paymentClient,
		ITakeNumberStore takeNumberStore,
		ISystemClock systemClock,
		ISerialNumberGenerator serialNumberGenerator)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
		_takeNumberStore = takeNumberStore ?? throw new ArgumentNullException(nameof(takeNumberStore));
		_systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		_serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
	}

	public async ValueTask<Guid> GetAsync(CreateWithdrawFormCommand command, CancellationToken cancellationToken = default)
	{
		var brandInfo = await _paymentRepository.GetBrandInfoByIdAsync(command.BrandId, cancellationToken);

		var orderNo = await _takeNumberStore.GetWithdrawFormOrderNoAsync(brandInfo.Code);
		var eventId = await _serialNumberGenerator
			.GenerateAsync(cancellationToken)
			.ConfigureAwait(false);
		var currentTime = _systemClock.UtcNow.UtcDateTime;

		var createFormId = Guid.NewGuid();
		var createFormSnapshot = WithdrawFormSnapshot._Default with
		{
			Id = createFormId,
			BrandId = command.BrandId,
			OrderNo = orderNo,
			UserId = command.User.UserId,
			UserName = command.User.UserName,
			UserType = command.User.UserType,
			Amount = command.Amount,
			Status = PaymentFlowState.Init,
			BankInfoId = command.BankInfoId,
			Note = command.Note,
			BankInfo = command.User.UserType switch
			{
				UserType.Player => await GetPlayerBankInfoContentAsync(
					command.User.UserId!,
					command.BankInfoId,
					cancellationToken).ConfigureAwait(false),
				UserType.Agent => await GetAgentBankInfoContentAsync(
					command.User.UserId!,
					command.BankInfoId,
					cancellationToken).ConfigureAwait(false),
				_ => throw new InvalidOperationException($"Can't handle user type: {command.User.UserType}")
			},
			CreatedOn = currentTime
		};

		var changeSets = new JsonPatch(PatchOperation.Add(
			JsonPointer.Create<WithdrawFormSnapshot>(o => o),
			JsonSerializer.SerializeToNode(createFormSnapshot, PaymentUtility.SerializerOptions)));

		await _paymentRepository.CreateWithdrawFormEventAsync(
			eventId,
			createFormId,
			EventAction.Create,
			JsonSerializer.SerializeToDocument(changeSets),
			null,
			cancellationToken).ConfigureAwait(false);

		await _paymentRepository.UpdateWithdrawFormSnapshotAsync(
			createFormSnapshot,
			eventId,
			currentTime,
			cancellationToken).ConfigureAwait(false);

		await _paymentClient
			.EmitWithdrawFormCreatedEventAsync(createFormId, command.BrandId, cancellationToken)
			.ConfigureAwait(false);

		return createFormId;
	}

	private async ValueTask<string> GetAgentBankInfoContentAsync(
		string userId,
		Guid bankInfoId,
		CancellationToken cancellationToken = default)
	{
		var agentId = Guid.Parse(userId);

		var agentBankInfo = await _paymentRepository
			.GetAgentBankInfoByIdAsync(bankInfoId, cancellationToken)
			.ConfigureAwait(false);

		if (agentBankInfo.AgentId != agentId)
			throw new InvalidOperationException("Can't find bank info.");

		var bankInfoData = new WithdrawBankInfo<JsonDocument>(
			agentBankInfo.Id,
			agentBankInfo.PaymentChannelId,
			agentBankInfo.BankType,
			JsonDocument.Parse(agentBankInfo.Detail));

		return JsonSerializer.Serialize(bankInfoData, PaymentUtility.SerializerOptions);
	}

	private async ValueTask<string> GetPlayerBankInfoContentAsync(
		string userId,
		Guid bankInfoId,
		CancellationToken cancellationToken = default)
	{
		var playerBankInfo = await _paymentRepository
			.GetBankInfoByIdAsync(bankInfoId, cancellationToken)
			.ConfigureAwait(false);

		if (playerBankInfo is null || playerBankInfo.PlayerCode != userId)
			throw new InvalidOperationException("Can't find bank info.");

		var bankInfoData = new WithdrawBankInfo<JsonDocument>(
			bankInfoId,
			playerBankInfo.PaymentChannelId,
			playerBankInfo.BankType,
			playerBankInfo.Detail);

		return JsonSerializer.Serialize(bankInfoData, PaymentUtility.SerializerOptions);
	}
}
