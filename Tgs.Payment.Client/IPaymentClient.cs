namespace Tgs.Payment.Clients;

public interface IPaymentClient
{
	ValueTask ApproveDepositAsync(Guid formId, Guid brandId, decimal? actualAmount);

	ValueTask<string> AskAsync(string subject, string requestInfo);

	ValueTask<string> DepositAsync(string subject, decimal amount, string? applyInfo);

	ValueTask EmitDepositFormCreatedEventAsync(Guid formId, Guid brandId);

	ValueTask EmitDepositFormStatusChangedEventAsync(Guid formId, Guid brandId, string status);

	ValueTask EmitWithdrawFormCreatedEventAsync(Guid fromId, Guid brandId);

	ValueTask EmitWithdrawFormStatusChangedEventAsync(Guid formId, Guid brandId, string status);

	ValueTask RejectDepositAsync(Guid formId, Guid brandId);

	ValueTask SaveWithdrawFormThirdPartyOrderNoAsync(Guid formId, string thirdPartyOrderNo);

	ValueTask ScheduleAsync(string subject, Guid formId, DateTime scheduleTime);

	ValueTask TagWithdrawFormPaidAsync(Guid formId);

	ValueTask<string> WithdrawAsync(string subject, decimal amount, string? applyInfo);
}
