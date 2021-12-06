namespace Tgs.Payment.ViewModels
{
	public class PagingWithdrawFormViewModel : PagingResponseViewModel<WithdrawFormViewModel>
	{
		public decimal Sum { get; init; }
	}
}
