using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices;
using TGS.ServiceUtility;

namespace Tgs.Payment.Controllers
{
	[Route("payment/method")]
	[ApiController]
	public class PaymentMethodController : ControllerBase
	{
		[HttpGet]
		public IAsyncEnumerable<PaymentMethodInfo> GetAllPaymentMethodsAsync(
			[FromServices] IQueryService<AllPaymentMethodsQuery, PaymentMethodInfo> paymentMethodService)
			=> paymentMethodService.QueryAsync(new AllPaymentMethodsQuery());
	}
}
