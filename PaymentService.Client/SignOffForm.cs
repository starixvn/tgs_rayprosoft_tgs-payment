using System;

namespace Tgs.Payment.Clients
{
	internal class SignOffForm
	{
		public Guid? BrandPaymentId { get; set; }

		public string? Remark { get; set; }

		public ReviewState ReviewState { get; set; }
	}
}
