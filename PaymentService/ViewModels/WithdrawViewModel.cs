namespace Tgs.Payment.ViewModels
{
	public class WithdrawViewModel : PagingResponseViewModel<WithdrawFormViewModel>
	{
		public decimal Sum { get; init; }
	}
}
