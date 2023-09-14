using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Json.Patch;
using Json.Pointer;
using Tgs.BrandCertificate.Clients;
using Tgs.Payment.Clients;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class CreateDepositFormCommandService : ICommandService<CreateDepositFormCommand>
{
	private readonly IPaymentClient _paymentClient;
	private readonly IBrandServiceClient _brandServiceClient;
	private readonly IPaymentRepository _paymentRepository;
	private readonly ISerialNumberGenerator _serialNumberGenerator;
	private readonly ITakeNumberStore _takeNumberStore;
	private readonly ISystemClock _systemClock;

	public CreateDepositFormCommandService(
		ISerialNumberGenerator serialNumberGenerator,
		IPaymentRepository paymentRepository,
		IPaymentClient paymentClient,
		IBrandServiceClient brandServiceClient,
		ITakeNumberStore takeNumberStore,
		ISystemClock systemClock)
	{
		_serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
		_brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));
		_takeNumberStore = takeNumberStore ?? throw new ArgumentNullException(nameof(takeNumberStore));
		_systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
	}

	public async ValueTask ExecuteAsync(CreateDepositFormCommand command, CancellationToken cancellationToken = default)
	{
		var brandInfo = await _brandServiceClient.GetBrandInfoAsync(
			command.CreateInfo.BrandId,
			cancellationToken).ConfigureAwait(false);

		var orderNo = await _takeNumberStore.GetDepositFormOrderNoAsync(brandInfo!.Code);

		var eventId = await _serialNumberGenerator
			.GenerateAsync(cancellationToken)
			.ConfigureAwait(false);

		var currentTime = _systemClock.UtcNow.UtcDateTime;
		var createSnapshot = new DepositFormSnapshot(
			command.FormId,
			command.CreateInfo.PlayerCode,
			command.CreateInfo.BrandId,
			command.CreateInfo.BrandPaymentId,
			command.CreateInfo.Amount,
			null,
			null,
			PaymentFlowState.WaitPayment,
			command.CreateInfo.ApplyInfo,
			command.CreateInfo.PaymentReturn,
			null,
			orderNo,
			null,
			null,
			null,
			command.CreateInfo.PromotionId,
			null,
			currentTime,
			0M);

		var changeSets = new JsonPatch(PatchOperation.Add(
			JsonPointer.Create<DepositFormSnapshot>(o => o),
			JsonSerializer.SerializeToNode(createSnapshot, PaymentUtility.SerializerOptions)));

		await _paymentRepository.CreateDepositFormEventAsync(
			eventId,
			command.FormId,
			EventAction.Create,
			JsonSerializer.SerializeToDocument(changeSets),
			null,
			cancellationToken).ConfigureAwait(false);

		await _paymentRepository.UpdateDepositFormSnapshotAsync(
			createSnapshot,
			eventId,
			currentTime,
			cancellationToken).ConfigureAwait(false);

		await _paymentClient
			.EmitDepositFormCreatedEventAsync(command.FormId, command.CreateInfo.BrandId, cancellationToken)
			.ConfigureAwait(false);
	}
}
