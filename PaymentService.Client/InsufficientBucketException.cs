using System;
using Tgs.Payment;

namespace Tgs.Payment.Clients;

public class InsufficientBucketException : Exception
{
	public InsufficientBucketException()
	{
	}
}
