using System;

namespace Tgs.Payment
{
	public record DepositFormCreateInfo
	{
		public string PlayerCode { get; init; } = default!;
		public Guid BrandId { get; init; }
		public Guid BrandPaymentId { get; init; }
		public decimal Amount { get; init; }
		public Guid? PromotionId { get; init; }
		public string? OrderNo { get; set; } = null;
		public string? ApplyInfo { get; init; }
		public string? PaymentReturn { get; init; } = null;
	}
}
