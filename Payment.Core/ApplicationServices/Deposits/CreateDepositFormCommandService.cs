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

internal class CreateDepositFormCommandService(
	ISerialNumberGenerator serialNumberGenerator,
	IPaymentRepository paymentRepository,
	IPaymentClient paymentClient,
	IBrandServiceClient brandServiceClient,
	ITakeNumberStore takeNumberStore,
	TimeProvider timeProvider)
	: ICommandService<CreateDepositFormCommand>
{
	public async ValueTask ExecuteAsync(CreateDepositFormCommand command, CancellationToken cancellationToken = default)
	{
		var brandInfo = await brandServiceClient.GetBrandInfoAsync(
			command.CreateInfo.BrandId,
			cancellationToken).ConfigureAwait(false);

		var orderNo = await takeNumberStore.GetDepositFormOrderNoAsync(brandInfo!.Code);

		var eventId = await serialNumberGenerator
			.GenerateAsync(cancellationToken)
			.ConfigureAwait(false);

		var currentTime = timeProvider.GetUtcNow().UtcDateTime;
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

		await paymentRepository.CreateDepositFormEventAsync(
			eventId,
			command.FormId,
			EventAction.Create,
			JsonSerializer.SerializeToDocument(changeSets),
			null,
			cancellationToken).ConfigureAwait(false);

		await paymentRepository.UpdateDepositFormSnapshotAsync(
			createSnapshot,
			eventId,
			currentTime,
			cancellationToken).ConfigureAwait(false);

		await paymentClient
			.EmitDepositFormCreatedEventAsync(command.FormId, command.CreateInfo.BrandId, cancellationToken)
			.ConfigureAwait(false);
	}
}
