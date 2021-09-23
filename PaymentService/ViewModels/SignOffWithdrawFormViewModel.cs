using System;

namespace Tgs.Payment.ViewModels
{
	public class SignOffWithdrawFormViewModel
    {
        public Guid? BrandPaymentId { get; set; }

        public string? Remark { get; set; }

        public ReviewState ReviewState { get; set; }
    }
}