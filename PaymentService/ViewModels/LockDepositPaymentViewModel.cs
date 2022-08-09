namespace Tgs.Payment.ViewModels;

public class LockDepositPaymentViewModel
{
	public Guid FormId { get; init; }

	public string PaymentCode { get; init; } = default!;

	public string PlayerCode { get; init; } = default!;
}
