using Tgs.Payment.Clients;
namespace Tgs.Payment;

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
