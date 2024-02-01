namespace Tgs.Payment.ViewModels;

public class SignOffWithdrawFormViewModel
{
	public Guid? BrandPaymentId { get; set; }

	public string? Remark { get; set; }

	public string? PlayerRemark { get; set; }

	public ReviewState ReviewState { get; set; }

	public string? ReviewUserId { get; set; }

	public string? ReviewUserName { get; set; }
}
