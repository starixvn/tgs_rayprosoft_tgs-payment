using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Json.Patch;
using Json.Pointer;
using Tgs.SerialNumberGenerator;

namespace Tgs.Payment;

internal class WithdrawService : IWithdrawService
{
	private readonly ICreateWithdrawFormPipe[] _createWithdrawFormPipes;
	private readonly IPaymentRepository _paymentRepository;
	private readonly ISerialNumberGenerator _serialNumberGenerator;
	private readonly ISystemClock _systemClock;
	private readonly ITakeNumberStore _takeNumberStore;

	public WithdrawService(
		IPaymentRepository paymentRepository,
		ITakeNumberStore takeNumberStore,
		ISystemClock systemClock,
		ISerialNumberGenerator serialNumberGenerator,
		IEnumerable<ICreateWithdrawFormPipe> createWithdrawFormPipes)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_takeNumberStore = takeNumberStore ?? throw new ArgumentNullException(nameof(takeNumberStore));
		_systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		_serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
		_createWithdrawFormPipes = (createWithdrawFormPipes ?? throw new ArgumentNullException(nameof(createWithdrawFormPipes)))
			.OrderByDescending(pipe => pipe.Order)
			.ToArray();
	}

	public async ValueTask<Guid> CreateWithdrawFormAsync(
		WithdrawUser user,
		Guid brandId,
		Guid bankInfoId,
		decimal amount,
		string? note,
		CancellationToken cancellationToken = default)
	{
		foreach (var pipe in _createWithdrawFormPipes)
			await pipe.BeforeCreateWithdrawFormAsync(
				user,
				brandId,
				bankInfoId,
				amount,
				note,
				cancellationToken).ConfigureAwait(false);

		var brandInfo = await _paymentRepository.GetBrandInfoByIdAsync(
			brandId,
			cancellationToken).ConfigureAwait(false);

		var orderNo = await _takeNumberStore.GetWithdrawFormOrderNoAsync(
			  brandInfo.Code).ConfigureAwait(false);

		var eventId = await _serialNumberGenerator.GenerateAsync(
			cancellationToken).ConfigureAwait(false);

		var currentTime = _systemClock.UtcNow.UtcDateTime;

		var createFormId = Guid.NewGuid();
		var createFormSnapshot = WithdrawFormSnapshot._Default with
		{
			Id = createFormId,
			BrandId = brandId,
			OrderNo = orderNo,
			UserId = user.UserId,
			UserName = user.UserName,
			UserType = user.UserType,
			Amount = amount,
			Status = PaymentFlowState.Init,
			BankInfoId = bankInfoId,
			Note = note,
			BankInfo = user.UserType switch
			{
				UserType.Player => await GetPlayerBankInfoContentAsync(
					user.UserId!,
					bankInfoId,
					cancellationToken).ConfigureAwait(false),
				UserType.Agent => await GetAgentBankInfoContentAsync(
					user.UserId!,
					bankInfoId,
					cancellationToken).ConfigureAwait(false),
				_ => throw new InvalidOperationException($"Can't handle user type: {user.UserType}")
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

		for (var i = _createWithdrawFormPipes.Length - 1; i >= 0; i--)
			await _createWithdrawFormPipes[i].AfterCreateWithdrawFormAsync(
				user,
				brandId,
				bankInfoId,
				amount,
				note,
				createFormId,
				cancellationToken).ConfigureAwait(false);

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
