using System;

namespace Tgs.Payment.ApplicationServices.BrandPayments
{
	public record QueryBrandPaymentQuery(Guid? BrandId, TransferType? TransferType, bool? Enabled, string? Keyword);
}
