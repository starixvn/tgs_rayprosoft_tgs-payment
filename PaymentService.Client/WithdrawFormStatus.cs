namespace Tgs.Payment.Clients
{
	internal class WithdrawFormStatus
	{
		public decimal? ActualAmount { get; init; }

		public string? PaymentReturn { get; init; }

		public PaymentFlowState Status { get; init; }
	}
}
