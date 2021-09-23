using System;

namespace Tgs.Payment.Clients
{
	public class PaymentChannelInfo
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = default!;
    }
}