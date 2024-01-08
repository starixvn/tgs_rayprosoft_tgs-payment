using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Json.Patch;
using Json.Pointer;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class UpdateBrandAutoWithdrawCommandService(
	IPaymentRepository paymentRepository,
	ISystemClock systemClock,
	ISerialNumberGenerator serialNumberGenerator) : ICommandService<UpdateBrandAutoWithdrawCommand>
{
	public async ValueTask ExecuteAsync(UpdateBrandAutoWithdrawCommand command, CancellationToken cancellationToken = default)
	{
		var currentTime = systemClock.UtcNow.UtcDateTime;

		var brandAutoInfo = await paymentRepository
			.GetBrandAutoWithdrawSettingAsync(command.BrandId, cancellationToken)
			.ConfigureAwait(false);

		var changeSets = default(JsonPatch);
		var snapshot = default(AutoWithdrawInfo);

		if (brandAutoInfo is null)
		{
			snapshot = new AutoWithdrawInfo(
				command.EnableAutoWithdraw,
				command.AmountUpperLimit,
				command.BrandPaymentIds);

			changeSets = new JsonPatch(PatchOperation.Add(
			JsonPointer.Create<AutoWithdrawInfo>(o => o),
			JsonSerializer.SerializeToNode(snapshot, PaymentUtility.SerializerOptions)));
		}
		else
		{
			var oldData = new AutoWithdrawInfo(
				brandAutoInfo.EnableAutoWithdraw,
				brandAutoInfo.AmountUpperLimit,
				brandAutoInfo.BrandPaymentIds);

			snapshot = oldData with
			{
				EnableAutoWithdraw = command.EnableAutoWithdraw,
				AmountUpperLimit = command.AmountUpperLimit,
				BrandPaymentIds = command.BrandPaymentIds,
			};

			changeSets = brandAutoInfo.CreatePatch(snapshot, PaymentUtility.SerializerOptions);
		}

		var eventId = await serialNumberGenerator
			.GenerateAsync(cancellationToken)
			.ConfigureAwait(false);

		await paymentRepository.CreateBrandAutoWithdrawEventAsync(
			eventId,
			command.BrandId,
			JsonSerializer.SerializeToDocument(changeSets),
			command.UserId,
			cancellationToken).ConfigureAwait(false);

		await paymentRepository.UpdateBrandAutoWithdrawSnapshotAsync(
			snapshot,
			command.BrandId,
			eventId,
			currentTime, cancellationToken).ConfigureAwait(false);
	}
}
