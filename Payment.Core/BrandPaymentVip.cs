using System;

namespace Tgs.Payment;

public record BrandPaymentVip(
	Guid BrandPaymentId,
	Guid VipLevelId);
