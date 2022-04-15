using System.Net.Http;
using NUnit.Framework;
using Tgs.Payment.Clients;

namespace PaymentService.Client.UnitTests;

public class WithdrawStateMachineTests
{
	[Test]
	public void Withdraw_Create_Form_Should_Transit_To_WaitApprove()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateWithdrawStateMachine();

		sut.Fire(PaymentTrigger.WithdrawCreate);

		Assert.AreEqual(PaymentFlowState.WaitApprove, sut.State);
	}

	[Test]
	public void Withdraw_State_Is_Init_When_Reject_Should_Transit_To_Reject()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateWithdrawStateMachine();

		sut.Fire(PaymentTrigger.Reject);

		Assert.AreEqual(PaymentFlowState.Reject, sut.State);
	}

	[Test]
	public void Withdraw_State_Is_WaitApprove_When_Approve_Should_Transit_To_Approved()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateWithdrawStateMachine(PaymentFlowState.WaitApprove);

		sut.Fire(PaymentTrigger.Approve);

		Assert.AreEqual(PaymentFlowState.Approved, sut.State);
	}

	[Test]
	public void Withdraw_State_Is_WaitApprove_When_Reject_Should_Transit_To_Reject()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateWithdrawStateMachine(PaymentFlowState.WaitApprove);

		sut.Fire(PaymentTrigger.Reject);

		Assert.AreEqual(PaymentFlowState.Reject, sut.State);
	}

	[Test]
	public void Withdraw_State_Is_Approved_When_Payment_Callback_Should_Transit_To_Paid()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateWithdrawStateMachine(PaymentFlowState.Approved);

		sut.Fire(PaymentTrigger.PaymentWithdrawCompleted);

		Assert.AreEqual(PaymentFlowState.Paid, sut.State);
	}

	[Test]
	public void Withdraw_State_Is_Approved_When_Reject_Should_Transit_To_Reject()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateWithdrawStateMachine(PaymentFlowState.Approved);

		sut.Fire(PaymentTrigger.Reject);

		Assert.AreEqual(PaymentFlowState.Reject, sut.State);
	}
}