using Stateless;

namespace Tgs.Payment.Clients;

public static class PaymentStateMachineFactory
{
	public static StateMachine<PaymentFlowState, PaymentTrigger> CreateDepositStateMachine(PaymentFlowState initialState = PaymentFlowState.Init)
	{
		var fsm = new StateMachine<PaymentFlowState, PaymentTrigger>(initialState);

		_ = fsm.Configure(PaymentFlowState.Init)
			.Permit(PaymentTrigger.DepositCreate, PaymentFlowState.WaitPayment)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		_ = fsm.Configure(PaymentFlowState.WaitPayment)
			.Permit(PaymentTrigger.PaymentDepositCompleted, PaymentFlowState.WaitApprove)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		_ = fsm.Configure(PaymentFlowState.WaitApprove)
			.Permit(PaymentTrigger.Approve, PaymentFlowState.Approved)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		return fsm;
	}

	public static StateMachine<PaymentFlowState, PaymentTrigger> CreateManualDepositStateMachine(PaymentFlowState initialState = PaymentFlowState.Init)
	{
		var fsm = new StateMachine<PaymentFlowState, PaymentTrigger>(initialState);

		_ = fsm.Configure(PaymentFlowState.Init)
			.Permit(PaymentTrigger.DepositCreate, PaymentFlowState.WaitPayment)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		_ = fsm.Configure(PaymentFlowState.WaitPayment)
			.Permit(PaymentTrigger.UploadPayCertificate, PaymentFlowState.WaitApprove)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		_ = fsm.Configure(PaymentFlowState.WaitApprove)
			.Permit(PaymentTrigger.Approve, PaymentFlowState.Approved)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		return fsm;
	}

	public static StateMachine<PaymentFlowState, PaymentTrigger> CreateWithdrawStateMachine(PaymentFlowState initialState = PaymentFlowState.Init)
	{
		var fsm = new StateMachine<PaymentFlowState, PaymentTrigger>(initialState);

		_ = fsm.Configure(PaymentFlowState.Init)
			.Permit(PaymentTrigger.WithdrawCreate, PaymentFlowState.WaitApprove)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		_ = fsm.Configure(PaymentFlowState.WaitApprove)
			.Permit(PaymentTrigger.Approve, PaymentFlowState.Approved)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		_ = fsm.Configure(PaymentFlowState.Approved)
			.Permit(PaymentTrigger.PaymentWithdrawCompleted, PaymentFlowState.Paid)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		return fsm;
	}
}
