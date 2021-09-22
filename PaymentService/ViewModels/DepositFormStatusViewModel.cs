namespace Tgs.Payment.ViewModels
{
	public class DepositFormStatusViewModel
	{
		public string? PaymentReturn { get; set; }

		public PaymentFlowState Status { get; set; }
	}
}