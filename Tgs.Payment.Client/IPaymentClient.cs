namespace Tgs.Payment.Clients;

public interface IPaymentClient
{
	ValueTask ApproveDepositAsync(Guid formId, Guid brandId, decimal? actualAmount, CancellationToken cancellationToken = default);

	ValueTask<string> AskAsync(string subject, string requestInfo, CancellationToken cancellationToken = default);

	ValueTask<string> DepositAsync(string subject, decimal amount, string? applyInfo, CancellationToken cancellationToken = default);

	ValueTask EmitDepositFormCreatedEventAsync(Guid formId, Guid brandId, CancellationToken cancellationToken = default);

	ValueTask EmitDepositFormStatusChangedEventAsync(Guid formId, Guid brandId, string status, CancellationToken cancellationToken = default);

	ValueTask EmitPaymentChannelCreatedEventAsync(string paymentChannelName, CancellationToken cancellationToken = default);

	ValueTask EmitPaymentCreatedEventAsync(string paymentCode, CancellationToken cancellationToken = default);

	ValueTask EmitPaymentMethodChannelChangedEventAsync(Guid pmcId, CancellationToken cancellationToken = default);

	ValueTask EmitPaymentMethodCreatedEventAsync(string paymentMethodName, CancellationToken cancellationToken = default);

	ValueTask EmitPlayerBankInfoAddedEventAsync(Guid brandId, Guid bankInfoId, CancellationToken cancellationToken = default);

	ValueTask EmitPlayerBankInfoDeletedEventAsync(Guid brandId, Guid bankInfoId, CancellationToken cancellationToken = default);

	ValueTask EmitWithdrawFormCreatedEventAsync(Guid fromId, Guid brandId, CancellationToken cancellationToken = default);

	ValueTask EmitWithdrawFormStatusChangedEventAsync(Guid formId, Guid brandId, string status, CancellationToken cancellationToken = default);

	ValueTask PaidWithdrawAsync(Guid formId, Guid brandId, CancellationToken cancellationToken = default);

	ValueTask RebuildPlayerBankInfoHashAsync(Guid brandId, Guid bankInfoId, CancellationToken cancellationToken = default);

	ValueTask RejectDepositAsync(Guid formId, Guid brandId, CancellationToken cancellationToken = default);

	ValueTask RejectWithdrawAsync(Guid formId, Guid brandId, CancellationToken cancellationToken = default);

	ValueTask SaveWithdrawFormThirdPartyOrderNoAsync(Guid formId, string thirdPartyOrderNo, CancellationToken cancellationToken = default);

	ValueTask ScheduleAsync(string subject, Guid formId, DateTime scheduleTime, CancellationToken cancellationToken = default);

	ValueTask<string> WithdrawAsync(string subject, decimal amount, string? applyInfo, CancellationToken cancellationToken = default);
}
