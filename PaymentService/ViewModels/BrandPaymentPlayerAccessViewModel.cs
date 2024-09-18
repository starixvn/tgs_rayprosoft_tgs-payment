using System;

namespace Tgs.Payment.ViewModels;

public class BrandPaymentPlayerAccessViewModel
{
    public IEnumerable<string> Blacklist { get; set; } = [];
    public IEnumerable<string> Whitelist { get; set;} = [];
}
