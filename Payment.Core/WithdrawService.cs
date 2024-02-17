using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Json.Patch;
using Json.Pointer;
using Tgs.BrandCertificate.Clients;
using Tgs.Payment.ApplicationServices.BrandPayments;
using Tgs.Payment.Clients;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.Payment;

internal class WithdrawService(
	IPaymentRepository paymentRepository,
	IBrandServiceClient brandServiceClient,
	ITakeNumberStore takeNumberStore,
	TimeProvider timeProvider,
	ISerialNumberGenerator serialNumberGenerator,
	IEnumerable<ICreateWithdrawFormPipe> createWithdrawFormPipes,
	IWithdrawProcessorFactory withdrawProcessorFactory,
	IGetService<CalculateHandlingFeeCommand, decimal> calculateService,
	IPaymentClient paymentClient,
	IEnumerable<IApproveWithdrawFormPipe> approveWithdrawFormPipes,
	IEnumerable<IRejectWithdrawFormPipe> rejectWithdrawFormPipes,
	IEnumerable<IPaidWithdrawFormPipe> paidWithdrawFormPipes)
	: IWithdrawService
{
	private readonly IApproveWithdrawFormPipe[] _approveWithdrawFormPipes = [
		.. (approveWithdrawFormPipes ?? throw new ArgumentNullException(nameof(approveWithdrawFormPipes)))
			.OrderByDescending(pipe => pipe.Order)];

	private readonly ICreateWithdrawFormPipe[] _createWithdrawFormPipes = [
		.. (createWithdrawFormPipes ?? throw new ArgumentNullException(nameof(createWithdrawFormPipes)))
			.OrderByDescending(pipe => pipe.Order)];

	private readonly IRejectWithdrawFormPipe[] _rejectWithdrawFormPipes = [
		.. (rejectWithdrawFormPipes ?? throw new ArgumentNullException(nameof(rejectWithdrawFormPipes)))
			.OrderByDescending(pipe => pipe.Order)];

	private readonly IPaidWithdrawFormPipe[] _paidWithdrawFormPipes = [
		.. (paidWithdrawFormPipes ?? throw new ArgumentNullException(nameof(paidWithdrawFormPipes)))
			.OrderByDescending(pipe => pipe.Order)];

	public async ValueTask ApproveWithdrawFormAsync(
		Guid brandPaymentId,
		WithdrawFormSnapshot formData,
		string? reviewUserId,
		string? reviewUserName,
		string? remark,
		string? playerRemark,
		CancellationToken cancellationToken = default)
	{
		var fsm = StateMachine.CreateWithdrawStateMachine(formData.Status);

		if (!fsm.CanFire(PaymentTrigger.Approve))
			return;

		await fsm.FireAsync(PaymentTrigger.Approve).ConfigureAwait(false);

		foreach (var pipe in _approveWithdrawFormPipes)
			await pipe.BeforeApproveWithdrawFormAsync(
				brandPaymentId,
				formData,
				reviewUserId,
				reviewUserName,
				remark,
				cancellationToken).ConfigureAwait(false);

		var brandPayment = await paymentRepository.GetBrandPaymentByIdAsync(
			brandPaymentId,
			cancellationToken).ConfigureAwait(false);

		var payment = await paymentRepository
			.GetPaymentAsync(brandPayment.PaymentId, cancellationToken)
			.ConfigureAwait(false);

		var processor = withdrawProcessorFactory.GetWithdrawProcessor(payment.Code);

		var handlingFee = await calculateService.GetAsync(
			new CalculateHandlingFeeCommand(brandPaymentId, formData.Amount),
			cancellationToken).ConfigureAwait(false);

		var withdrawSettings = processor.MakeWithdrawSettings(
			formData.Id,
			formData.Amount,
			formData.BankInfo,
			brandPayment);

		var applyInfo = await paymentClient
			.WithdrawAsync(
				processor.Subject,
				formData.Amount - handlingFee,
				withdrawSettings,
				cancellationToken)
			.ConfigureAwait(false);

		var currentTime = timeProvider.GetUtcNow().UtcDateTime;
		var updateFormData = formData with
		{
			BrandPaymentId = brandPaymentId,
			ApplyInfo = applyInfo,
			Status = fsm.State,
			ReviewedUserId = reviewUserId,
			ReviewedUserName = reviewUserName,
			ReviewedOn = currentTime,
			Remark = remark,
			PlayerRemark = playerRemark,
			HandlingFee = handlingFee
		};

		var changeSets = formData.CreatePatch(updateFormData, PaymentUtility.SerializerOptions);
		var eventId = await serialNumberGenerator
			.GenerateAsync(cancellationToken)
			.ConfigureAwait(false);

		await paymentRepository.CreateWithdrawFormEventAsync(
			eventId,
			formData.Id,
			EventAction.Review,
			JsonSerializer.SerializeToDocument(changeSets),
			reviewUserId,
			cancellationToken).ConfigureAwait(false);

		await paymentRepository.UpdateWithdrawFormSnapshotAsync(
			updateFormData,
			eventId,
			currentTime,
			cancellationToken).ConfigureAwait(false);

		for (var i = _approveWithdrawFormPipes.Length - 1; i >= 0; i--)
			await _approveWithdrawFormPipes[i].AfterApproveWithdrawFormAsync(
				brandPaymentId,
				formData,
				reviewUserId,
				reviewUserName,
				remark,
				cancellationToken).ConfigureAwait(false);
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

		var brandInfo = await brandServiceClient.GetBrandInfoAsync(
			brandId,
			cancellationToken).ConfigureAwait(false);

		var orderNo = await takeNumberStore.GetWithdrawFormOrderNoAsync(
			  brandInfo!.Code).ConfigureAwait(false);

		var eventId = await serialNumberGenerator.GenerateAsync(
			cancellationToken).ConfigureAwait(false);

		var currentTime = timeProvider.GetUtcNow().UtcDateTime;

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

		await paymentRepository.CreateWithdrawFormEventAsync(
			eventId,
			createFormId,
			EventAction.Create,
			JsonSerializer.SerializeToDocument(changeSets),
			null,
			cancellationToken).ConfigureAwait(false);

		await paymentRepository.UpdateWithdrawFormSnapshotAsync(
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

	public async ValueTask PaidWithdrawFormAsync(
		WithdrawFormSnapshot formData,
		string? reviewUserId,
		string? reviewUserName,
		string? remark,
		string? playerRemark,
		CancellationToken cancellationToken = default)
	{
		var fsm = StateMachine.CreateWithdrawStateMachine(formData.Status);

		if (!fsm.CanFire(PaymentTrigger.PaymentWithdrawCompleted))
			return;

		await fsm.FireAsync(PaymentTrigger.PaymentWithdrawCompleted).ConfigureAwait(false);

		foreach (var pipe in _paidWithdrawFormPipes)
			await pipe.BeforePaidWithdrawFormAsync(
				formData,
				reviewUserId,
				reviewUserName,
				remark,
				cancellationToken).ConfigureAwait(false);

		var currentTime = timeProvider.GetUtcNow().UtcDateTime;
		var updateFormData = formData with
		{
			Status = fsm.State,
			PaidUserId = reviewUserId,
			PaidUserName = reviewUserName,
			PaidOn = currentTime,
			PlayerRemark = playerRemark,
		};

		var changeSets = formData.CreatePatch(updateFormData, PaymentUtility.SerializerOptions);
		var eventId = await serialNumberGenerator
			.GenerateAsync(cancellationToken)
			.ConfigureAwait(false);

		await paymentRepository.CreateWithdrawFormEventAsync(
			eventId,
			formData.Id,
			EventAction.Review,
			JsonSerializer.SerializeToDocument(changeSets),
			reviewUserId,
			cancellationToken).ConfigureAwait(false);

		await paymentRepository.UpdateWithdrawFormSnapshotAsync(
			updateFormData,
			eventId,
			currentTime,
			cancellationToken).ConfigureAwait(false);

		for (var i = _paidWithdrawFormPipes.Length - 1; i >= 0; i--)
			await _paidWithdrawFormPipes[i].AfterPaidWithdrawFormAsync(
				formData,
				reviewUserId,
				reviewUserName,
				remark,
				cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask RejectWithdrawFormAsync(
		WithdrawFormSnapshot formData,
		string? reviewUserId,
		string? reviewUserName,
		string? remark,
		string? playerRemark,
		CancellationToken cancellationToken = default)
	{
		var fsm = StateMachine.CreateWithdrawStateMachine(formData.Status);

		if (!fsm.CanFire(PaymentTrigger.Reject))
			return;

		await fsm.FireAsync(PaymentTrigger.Reject).ConfigureAwait(false);

		foreach (var pipe in _rejectWithdrawFormPipes)
			await pipe.BeforeRejectWithdrawFormAsync(
				formData,
				reviewUserId,
				reviewUserName,
				remark,
				cancellationToken).ConfigureAwait(false);

		var currentTime = timeProvider.GetUtcNow().UtcDateTime;

		var updateFormData = formData with
		{
			Status = fsm.State,
			ReviewedUserId = reviewUserId,
			ReviewedUserName = reviewUserName,
			ReviewedOn = currentTime,
			Remark = remark,
			PlayerRemark = playerRemark,
		};

		if (remark is null)
		{
			updateFormData = formData with
			{
				Status = fsm.State,
				ReviewedUserId = reviewUserId,
				ReviewedUserName = reviewUserName,
				ReviewedOn = currentTime,
				PlayerRemark = playerRemark,
			};
		}

		var changeSets = formData.CreatePatch(updateFormData, PaymentUtility.SerializerOptions);
		var eventId = await serialNumberGenerator
			.GenerateAsync(cancellationToken)
			.ConfigureAwait(false);

		await paymentRepository.CreateWithdrawFormEventAsync(
			eventId,
			formData.Id,
			EventAction.Review,
			JsonSerializer.SerializeToDocument(changeSets),
			reviewUserId,
			cancellationToken).ConfigureAwait(false);

		await paymentRepository.UpdateWithdrawFormSnapshotAsync(
			updateFormData,
			eventId,
			currentTime,
			cancellationToken).ConfigureAwait(false);

		for (var i = _rejectWithdrawFormPipes.Length - 1; i >= 0; i--)
			await _rejectWithdrawFormPipes[i].AfterRejectWithdrawFormAsync(
				formData,
				reviewUserId,
				reviewUserName,
				remark,
				cancellationToken).ConfigureAwait(false);
	}

	private async ValueTask<string> GetAgentBankInfoContentAsync(
		string userId,
		Guid bankInfoId,
		CancellationToken cancellationToken = default)
	{
		var agentId = Guid.Parse(userId);

		var agentBankInfo = await paymentRepository
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
		var playerBankInfo = await paymentRepository
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
