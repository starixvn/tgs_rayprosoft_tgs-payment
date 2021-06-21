using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tgs.Payment.Controllers
{
	[Route("payment/method")]
	[ApiController]
	public class PaymentMethodController : ControllerBase
	{
		public void GetAllPaymentMethods()
		{

		}
	}
}
