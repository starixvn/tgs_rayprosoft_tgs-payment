using System;

namespace Tgs.Payment.ViewModels
{
	public class BrandPaymentAclViewModel
	{
		public Guid BrandPaymentId { get; set; }

		public bool IsBlock { get; set; }

		public string PlayerCode { get; set; } = default!;
	}
}
