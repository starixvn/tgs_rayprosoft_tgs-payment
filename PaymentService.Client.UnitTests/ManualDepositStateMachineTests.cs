using System.Net.Http;
using NUnit.Framework;
using Tgs.Payment.Clients;

namespace PaymentService.Client.UnitTests;

public class ManualDepositStateMachineTests
{
	[Test]
	public void ManualDeposit_Create_Form_Should_Transit_To_WaitPayment()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateManualDepositStateMachine();

		sut.Fire(PaymentTrigger.DepositCreate);

		Assert.AreEqual(PaymentFlowState.WaitPayment, sut.State);
	}

	[Test]
	public void ManualDeposit_State_Is_Init_When_Reject_Should_Transit_To_Reject()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateManualDepositStateMachine(PaymentFlowState.Init);

		sut.Fire(PaymentTrigger.Reject);

		Assert.AreEqual(PaymentFlowState.Reject, sut.State);
	}

	[Test]
	public void ManualDeposit_State_Is_WaitApprove_When_Approve_Should_Transit_To_Approved()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateManualDepositStateMachine(PaymentFlowState.WaitApprove);

		sut.Fire(PaymentTrigger.Approve);

		Assert.AreEqual(PaymentFlowState.Approved, sut.State);
	}

	[Test]
	public void ManualDeposit_State_Is_WaitApprove_When_Reject_Should_Transit_To_Reject()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateManualDepositStateMachine(PaymentFlowState.WaitApprove);

		sut.Fire(PaymentTrigger.Reject);

		Assert.AreEqual(PaymentFlowState.Reject, sut.State);
	}

	[Test]
	public void ManualDeposit_State_Is_WaitPayment_Can_Not_Approve()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateManualDepositStateMachine();

		Assert.IsFalse(sut.CanFire(PaymentTrigger.Approve));
	}

	[Test]
	public void ManualDeposit_State_Is_WaitPayment_Upload_CertificateFile_Should_Transit_To_WaitApprove()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateManualDepositStateMachine(PaymentFlowState.WaitPayment);

		sut.Fire(PaymentTrigger.UploadPayCertificate);

		Assert.AreEqual(PaymentFlowState.WaitApprove, sut.State);
	}

	[Test]
	public void ManualDeposit_State_Is_WaitPayment_When_Reject_Should_Transit_To_Reject()
	{
		var client = new PaymentServiceClient(new HttpClient());

		var sut = client.CreateManualDepositStateMachine(PaymentFlowState.WaitPayment);

		sut.Fire(PaymentTrigger.Reject);

		Assert.AreEqual(PaymentFlowState.Reject, sut.State);
	}
}
