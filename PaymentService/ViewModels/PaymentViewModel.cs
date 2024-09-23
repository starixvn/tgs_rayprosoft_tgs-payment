namespace Tgs.Payment.ViewModels;

public class PaymentViewModel
{
	public string Code { get; set; } = default!;

	public Guid Id { get; set; }

	public string Description { get; set; } = default!;
}
