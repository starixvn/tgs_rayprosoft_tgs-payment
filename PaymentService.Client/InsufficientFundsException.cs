using System;
using Tgs.Payment;

namespace Tgs.Payment.Clients;

public class InsufficientFundsException : Exception
{
	public InsufficientFundsException()
	{
	}
}
