namespace Tgs.Payment.ViewModels;

public class CreateWithdrawFormViewModel
{
	public decimal Amount { get; set; }

	public Guid BankInfoId { get; set; }

	public Guid BrandId { get; set; }

	public string? Note { get; set; }

	public string? UserId { get; set; }

	public string? UserName { get; set; }

	public UserType UserType { get; set; }
}
