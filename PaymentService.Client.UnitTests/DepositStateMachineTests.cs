using System.Net.Http;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Tgs.Payment.Clients;

namespace PaymentService.Client.UnitTests;

public class DepositStateMachineTests
{
	[Test]
	public void Deposit_Create_Form_Should_Transit_To_WaitPayment()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateDepositStateMachine();

		sut.Fire(PaymentTrigger.DepositCreate);

		ClassicAssert.AreEqual(PaymentFlowState.WaitPayment, sut.State);
	}

	[Test]
	public void Deposit_State_Is_Init_When_Reject_Should_Transit_To_Reject()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateDepositStateMachine(PaymentFlowState.Init);

		sut.Fire(PaymentTrigger.Reject);

		ClassicAssert.AreEqual(PaymentFlowState.Reject, sut.State);
	}

	[Test]
	public void Deposit_State_Is_WaitApprove_When_Approve_Should_Transit_To_Approved()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateDepositStateMachine(PaymentFlowState.WaitApprove);

		sut.Fire(PaymentTrigger.Approve);

		ClassicAssert.AreEqual(PaymentFlowState.Approved, sut.State);
	}

	[Test]
	public void Deposit_State_Is_WaitApprove_When_Reject_Should_Transit_To_Reject()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateDepositStateMachine(PaymentFlowState.WaitApprove);

		sut.Fire(PaymentTrigger.Reject);

		ClassicAssert.AreEqual(PaymentFlowState.Reject, sut.State);
	}

	[Test]
	public void Deposit_State_Is_WaitPayment_When_Payment_Callback_Should_Transit_To_WaitApprove()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateDepositStateMachine(PaymentFlowState.WaitPayment);

		sut.Fire(PaymentTrigger.PaymentDepositCompleted);

		ClassicAssert.AreEqual(PaymentFlowState.WaitApprove, sut.State);
	}

	[Test]
	public void Deposit_State_Is_WaitPayment_When_Reject_Should_Transit_To_Reject()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateDepositStateMachine(PaymentFlowState.WaitPayment);

		sut.Fire(PaymentTrigger.Reject);

		ClassicAssert.AreEqual(PaymentFlowState.Reject, sut.State);
	}
}
