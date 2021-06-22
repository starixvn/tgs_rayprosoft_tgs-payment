using System;

namespace Tgs.Payment.Clients
{
	public record BrandPaymentVip(
		Guid BrandPaymentId,
		Guid VipLevelId);
}
