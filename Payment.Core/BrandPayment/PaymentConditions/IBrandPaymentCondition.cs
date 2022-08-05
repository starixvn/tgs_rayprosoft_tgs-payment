using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tgs.Payment.BrandPayment.PaymentConditions;


public interface IBrandPaymentCondition
{
	bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player, decimal depositedAmount);
}

