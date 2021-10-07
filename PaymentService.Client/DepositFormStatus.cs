namespace Tgs.Payment.Clients
{
	internal class DepositFormStatus
	{
		public string? PaymentReturn { get; set; }

		public PaymentFlowState Status { get; set; }
	}
}