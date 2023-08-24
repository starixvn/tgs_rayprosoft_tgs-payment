using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Json.Patch;
using Json.Pointer;
using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class UpdateBrandAutoWithdrawCommandService : ICommandService<UpdateBrandAutoWithdrawCommand>
{
	private readonly IPaymentRepository _paymentRepository;
	private readonly ISerialNumberGenerator _serialNumberGenerator;
	private readonly ISystemClock _systemClock;

	public UpdateBrandAutoWithdrawCommandService(
		IPaymentRepository paymentRepository,
		ISystemClock systemClock,
		ISerialNumberGenerator serialNumberGenerator)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		_serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
	}

	public async ValueTask ExecuteAsync(UpdateBrandAutoWithdrawCommand command, CancellationToken cancellationToken = default)
	{
		var currentTime = _systemClock.UtcNow.UtcDateTime;

		var brandAutoInfo = await _paymentRepository
			.GetBrandAutoWithdrawSettingAsync(command.BrandId, cancellationToken)
			.ConfigureAwait(false);

		var changeSets = default(JsonPatch);
		var snapshot = default(BrandAutoWithdrawSnapshot);

		if (brandAutoInfo is null)
		{
			snapshot = new BrandAutoWithdrawSnapshot(
				command.EnableAutoWithdraw,
				command.AmountUpperLimit,
				command.BrandPaymentCandidates);

			changeSets = new JsonPatch(PatchOperation.Add(
			JsonPointer.Create<BrandAutoWithdrawSnapshot>(o => o),
			JsonSerializer.SerializeToNode(snapshot, PaymentUtility.SerializerOptions)));
		}
		else
		{
			var oldData = new BrandAutoWithdrawSnapshot(
				brandAutoInfo.EnableAutoWithdraw,
				brandAutoInfo.AmountUpperLimit,
				JsonDocument.Parse(brandAutoInfo.BrandPaymentCandidates));

			snapshot = oldData with
			{
				EnableAutoWithdraw = command.EnableAutoWithdraw,
				AmountUpperLimit = command.AmountUpperLimit,
				BrandPaymentCandidates = command.BrandPaymentCandidates,
			};

			changeSets = brandAutoInfo.CreatePatch(snapshot, PaymentUtility.SerializerOptions);
		}

		var eventId = await _serialNumberGenerator
			.GenerateAsync(cancellationToken)
			.ConfigureAwait(false);

		await _paymentRepository.CreateBrandAutoWithdrawEventAsync(
			eventId,
			command.BrandId,
			JsonSerializer.SerializeToDocument(changeSets),
			command.UserId,
			cancellationToken).ConfigureAwait(false);

		await _paymentRepository.UpdateBrandAutoWithdrawSnapshotAsync(
			snapshot,
			command.BrandId,
			eventId,
			currentTime, cancellationToken).ConfigureAwait(false);
	}
}
