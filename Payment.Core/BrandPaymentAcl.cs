using System;

namespace Tgs.Payment
{
	public record BrandPaymentAcl(
        Guid BrandPaymentId,
        string PlayerCode,
        bool IsBlock);
}