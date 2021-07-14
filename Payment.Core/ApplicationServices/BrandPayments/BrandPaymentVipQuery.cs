using System;
using System.Collections.Generic;

namespace Tgs.Payment.ApplicationServices.BrandPayments
{
	public record BrandPaymentVipQuery(IEnumerable<Guid> BrandPaymentIds);
}
