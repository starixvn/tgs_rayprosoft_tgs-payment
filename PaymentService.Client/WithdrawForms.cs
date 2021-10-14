namespace Tgs.Payment.Clients
{
	public class WithdrawForms : PagingResponse<WithdrawFormInfo>
	{
		public decimal Sum { get; init; }
	}
}
