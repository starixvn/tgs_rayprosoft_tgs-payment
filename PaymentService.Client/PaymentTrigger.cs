namespace Tgs.Payment.Clients;

public enum PaymentTrigger
{
	Approve,
	Reject,
	DepositCreate,
	WithdrawCreate,
	UploadPayCertificate,
	PaymentDepositCompleted,
	PaymentWithdrawCompleted
}
