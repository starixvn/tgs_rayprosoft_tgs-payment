using System;

namespace Tgs.Payment.Clients
{
	public record PaymentMethodChannelInfo(
		Guid Id,
		Guid PaymentId,
		Guid MethodId,
		Guid ChannelId);
}
