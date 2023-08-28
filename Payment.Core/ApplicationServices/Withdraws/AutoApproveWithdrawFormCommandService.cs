using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws;

internal class AutoApproveWithdrawFormCommandService : ICommandService<AutoApproveWithdtawFormCommand>
{
	private static readonly Random _Random = new();
	private readonly ICommandService<SignOffWithdrawFormCommand> _commandService;
	private readonly IPaymentRepository _repository;

	public AutoApproveWithdrawFormCommandService(
		IPaymentRepository repository,
		ICommandService<SignOffWithdrawFormCommand> commandService)
	{
		_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
	}

	public async ValueTask ExecuteAsync(AutoApproveWithdtawFormCommand command, CancellationToken cancellationToken = default)
	{
		var withdrawForm = await _repository
			.GetWithdrawFormByIdAsync(command.FormId, cancellationToken)
			.ConfigureAwait(false);

		if (withdrawForm is null)
			return;

		if (withdrawForm.UserType is not UserType.Player || withdrawForm.UserId is null)
			return;

		var playerBankInfo = await _repository
			.GetBankInfosByPlayerCodeAsync(withdrawForm.UserId, cancellationToken)
			.FirstOrDefaultAsync(b => b.Id == withdrawForm.BankInfoId, cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (playerBankInfo is null)
			return;

		var couldChooseBrandPayments = await _repository
			.GetBrandPaymentsAsync(command.BrandId, cancellationToken)
			.Where(bp => bp.ChannelId == playerBankInfo.PaymentChannelId && bp.TransferType == TransferType.Withdraw && bp.Enabled == true)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		if (couldChooseBrandPayments == null || couldChooseBrandPayments.Any() == false)
			return;

		var autoWithdrawSetting = await _repository
			.GetBrandAutoWithdrawSettingAsync(command.BrandId, cancellationToken)
			.ConfigureAwait(false);

		if (autoWithdrawSetting is null)
			return;

		if (!autoWithdrawSetting.EnableAutoWithdraw)
			return;

		if (withdrawForm.Amount > autoWithdrawSetting.AmountUpperLimit)
			return;

		var brandPaymentCandidates = autoWithdrawSetting.BrandPaymentIds.Join(
			couldChooseBrandPayments,
			settingBrandPayment => settingBrandPayment,
			couldChoose => couldChoose.Id,
			(setting, couldChoose) => setting);

		var random = _Random.Next(0, brandPaymentCandidates.Count() - 1);

		var brandPaymentId = brandPaymentCandidates.ElementAt(random);

		var approveWithdrawCommand = new SignOffWithdrawFormCommand(
			command.FormId,
			brandPaymentId,
			ReviewState.Approve,
			null,
			null,
			"System Auto Approve");

		await _commandService
			.ExecuteAsync(approveWithdrawCommand, cancellationToken)
			.ConfigureAwait(false);
	}
}
