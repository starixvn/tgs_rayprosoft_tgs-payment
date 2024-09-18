using System;
using System.Collections.Generic;

namespace Tgs.Payment;

public record BrandPaymentPlayerAccess(
    IEnumerable<string> Blacklist,
    IEnumerable<string> Whitelist
);