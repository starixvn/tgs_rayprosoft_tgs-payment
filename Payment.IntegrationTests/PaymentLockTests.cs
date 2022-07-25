using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Payment.IntegrationTests;
public class PaymentLockTests
{
	[Test]
	public async Task LockPayment()
	{
		await using var web = TestSiteHelper.GetPaymentServiceTestHost();
	}
}
