namespace Tgs.Payment.Clients;

public class DepositForms : PagingResponse<DepositFormInfo>
{
	public decimal Sum { get; init; }
}
