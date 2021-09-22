using System;

namespace Tgs.Payment.ViewModels
{
	public class PaymentChannelViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = default!;
    }
}