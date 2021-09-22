using System;

namespace Tgs.Payment.ViewModels
{
	public class PaymentMethodViewModel
    {
        public string BankType { get; set; } = default!;

        public Guid Id { get; set; }

        public string Name { get; set; } = default!;
    }
}