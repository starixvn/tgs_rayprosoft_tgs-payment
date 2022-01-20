using System;

namespace Tgs.Payment;

public record PaymentMethodChannelInfo(
	Guid Id,
	Guid PaymentId,
	Guid MethodId,
	Guid ChannelId);
