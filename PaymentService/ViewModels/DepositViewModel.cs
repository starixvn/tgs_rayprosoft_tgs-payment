namespace Tgs.Payment.ViewModels
{
	public class DepositViewModel
	{
		public PagingResponseViewModel<DepositFormViewModel>? Datas { get; init; }

		public decimal Sum { get; init; }
	}
}
