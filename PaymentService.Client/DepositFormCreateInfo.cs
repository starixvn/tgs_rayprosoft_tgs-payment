using System;

namespace Tgs.Payment.Clients
{
	public class DepositFormCreateInfo
    {
        public decimal Amount { get; set; }

        public string ApplyInfo { get; set; } = default!;

        public Guid BrandId { get; set; }

        public Guid BrandPaymentId { get; set; }

        public string PlayerCode { get; set; } = default!;

        public Guid? PromotionId { get; set; }
    }
}