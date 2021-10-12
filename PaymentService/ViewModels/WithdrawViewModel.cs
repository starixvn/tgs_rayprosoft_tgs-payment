namespace Tgs.Payment.ViewModels
{
	public class WithdrawViewModel
	{
		public PagingResponseViewModel<WithdrawFormViewModel>? Datas { get; init; }

		public decimal Sum { get; init; }
	}
}
