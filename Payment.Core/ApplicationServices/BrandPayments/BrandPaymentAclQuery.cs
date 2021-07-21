using System;
using System.Collections.Generic;

namespace Tgs.Payment.ApplicationServices.BrandPayments
{
	public record BrandPaymentAclQuery(string PlayerCode, IEnumerable<Guid> BrandPaymentIds);
}
