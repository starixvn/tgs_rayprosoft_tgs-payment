using System;

namespace Tgs.Payment.Clients
{
	public record BrandPaymentAcl(
		Guid BrandPaymentId,
		string PlayerCode,
		bool IsBlock);
}
