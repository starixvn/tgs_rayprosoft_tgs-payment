namespace Tgs.Payment;

public enum PaymentFlowState
{
	Init = 0,
	WaitPayment = 100,
	WaitApprove = 1000,
	Reject = 7000,
	Approved = 8000,
	Paid = 9000
}
