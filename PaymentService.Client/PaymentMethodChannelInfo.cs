namespace Tgs.Payment.Clients;

public class PaymentMethodChannelInfo
{
	public Guid ChannelId { get; init; }

	public Guid Id { get; init; }

	public Guid MethodId { get; init; }

	public Guid PaymentId { get; init; }
}
