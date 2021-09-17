using System;

namespace Tgs.Payment.ViewModels
{
	public class DepositFormViewModel
	{
		public decimal Amount { get; set; }

		public string? ApplyInfo { get; set; }

		public Guid BrandId { get; set; }

		public Guid BrandPaymentId { get; set; }

		public string ChannelName { get; set; } = default!;

		public DateTimeOffset CreatedOn { get; set; }

		public Guid Id { get; set; }

		public Guid PaymentMethodId { get; set; }

		public string? PaymentReturn { get; set; }

		public string PlayerCode { get; set; } = default!;

		public Guid? PromotionId { get; set; }

		public string? Remark { get; set; }

		public PaymentFlowState Status { get; set; }

		public DateTimeOffset UpdatedOn { get; set; }

		public string UserName { get; set; } = default!;

		public string? ThirdPartyOrderNo { get; set; }
	}
}
