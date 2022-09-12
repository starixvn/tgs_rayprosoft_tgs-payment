using System;
using System.Collections.Generic;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

public record GetBrandPaymentFeesQuery(IEnumerable<Guid> BrandPaymentIds);
