using System;

namespace Tgs.Payment.Clients;

public class BrandPaymentPlayerAccess
{
    public IEnumerable<string> Blacklist { get; set; } = default!;
    public IEnumerable<string> Whitelist { get; set; } = default!;
}
