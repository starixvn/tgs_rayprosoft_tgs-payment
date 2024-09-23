using System;

namespace Tgs.Payment;

public record PaymentInfo(
	Guid Id,
	string Code,
	string? Description);
