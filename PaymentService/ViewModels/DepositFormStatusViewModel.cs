namespace Tgs.Payment.ViewModels
{
	public class DepositFormStatusViewModel
	{
		public decimal? ActualAmount { get; set; }

		public string? PaymentReturn { get; set; }

		public PaymentFlowState Status { get; set; }

		public string? ThirdPartyOrderNo { get; set; }
	}
}
