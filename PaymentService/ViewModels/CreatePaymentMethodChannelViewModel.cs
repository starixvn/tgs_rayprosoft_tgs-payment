namespace Tgs.Payment.ViewModels;

public class CreatePaymentMethodChannelViewModel
{
	public Guid ChannelId { get; set; }

	public Guid MethodId { get; set; }

	public Guid PaymentId { get; set; }
}
