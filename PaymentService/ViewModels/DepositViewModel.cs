namespace Tgs.Payment.ViewModels
{
	public class DepositViewModel: PagingResponseViewModel<DepositFormViewModel>
	{
		public decimal Sum { get; init; }
	}
}
