using System;

namespace Tgs.Payment.Clients
{
	public record PaymentMethodInfo(Guid Id, string Name, string BankType);
}
