using System;

namespace Tgs.Payment
{
	public record PaymentMethodInfo(Guid Id, string Name, string BankType);
}
