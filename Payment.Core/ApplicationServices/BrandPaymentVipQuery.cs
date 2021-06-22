using System;
using System.Collections.Generic;

namespace Tgs.Payment.ApplicationServices
{
	public record BrandPaymentVipQuery(IEnumerable<Guid> BrandPaymentIds);
}
