using System;

namespace Tgs.Payment.ViewModels
{
	public class PaymentMethodChannelViewModel
	{
		public Guid ChannelId { get; set; }

		public Guid Id { get; set; }

		public Guid MethodId { get; set; }

		public Guid PaymentId { get; set; }
	}
}
