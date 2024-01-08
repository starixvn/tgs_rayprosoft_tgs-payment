using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class AutoApproveWithdrawFormCommandService(
	IPaymentRepository repository,
	ICommandService<SignOffWithdrawFormCommand> commandService,
	ILogger<AutoApproveWithdrawFormCommandService> logger)
	: ICommandService<AutoApproveWithdtawFormCommand>
{
	private static readonly Random _Random = new();

	public async ValueTask ExecuteAsync(AutoApproveWithdtawFormCommand command, CancellationToken cancellationToken = default)
	{
		if (command.Status != PaymentFlowState.Init)
			return;

		var autoWithdrawSetting = await repository
			.GetBrandAutoWithdrawSettingAsync(command.BrandId, cancellationToken)
			.ConfigureAwait(false);

		if (autoWithdrawSetting is null)
			return;

		if (autoWithdrawSetting.EnableAutoWithdraw is false || autoWithdrawSetting.BrandPaymentIds.Count() is 0)
			return;

		var withdrawForm = await repository
			.GetWithdrawFormByIdAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		if (withdrawForm is null)
			return;

		var fsm = StateMachine.CreateWithdrawStateMachine(withdrawForm.Status);
		if (!fsm.CanFire(PaymentTrigger.Approve))
			return;

		if (withdrawForm.UserType is not UserType.Player)
			return;

		if (withdrawForm.Amount > autoWithdrawSetting.AmountUpperLimit)
			return;

		var withdrawBankInfo = JsonSerializer.Deserialize<WithdrawBankInfo<object>>(withdrawForm.BankInfo!, PaymentUtility.SerializerOptions);

		var couldChooseBrandPayments = await repository
			.GetBrandPaymentsAsync(command.BrandId, cancellationToken)
			.Where(bp => bp.ChannelId == withdrawBankInfo!.PaymentChannelId && bp.TransferType == TransferType.Withdraw && bp.Enabled == true)
			.Select(x => x.Id)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		logger.LogDebug(
			"Auto Approve Withdtaw Form Command could choose BrandPayment Ids: {couldChooseBrandPayments}",
			JsonSerializer.Serialize(couldChooseBrandPayments, PaymentUtility.SerializerOptions));

		var brandPaymentCandidates = couldChooseBrandPayments.Intersect(autoWithdrawSetting.BrandPaymentIds);

		if (brandPaymentCandidates.Count() is 0)
		{
			logger.LogWarning("System auto approve brand payment candidates Count is 0.");
			return;
		}

		var random = _Random.Next(0, brandPaymentCandidates.Count());

		var brandPaymentId = brandPaymentCandidates.ElementAt(random);

		logger.LogDebug("Auto Approve Withdtaw Form Command Select BrandPayment Id:{brandPaymentId}", brandPaymentId);

		var approveWithdrawCommand = new SignOffWithdrawFormCommand(
			command.FormId,
			brandPaymentId,
			ReviewState.Approve,
			null,
			null,
			"System Auto Approve");

		await commandService
			.ExecuteAsync(approveWithdrawCommand, cancellationToken)
			.ConfigureAwait(false);
	}
}
