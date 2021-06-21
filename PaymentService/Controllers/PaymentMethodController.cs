using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices;
using Tgs.Payment.ViewModels;
using TGS.ServiceUtility;

namespace Tgs.Payment.Controllers
{
	[Route("payment/method")]
	[ApiController]
	public class PaymentMethodController : ControllerBase
	{
		[HttpGet]
		public IAsyncEnumerable<PaymentMethodViewModel> GetAllPaymentMethodsAsync(
			[FromServices] IQueryService<AllPaymentMethodsQuery, PaymentMethodInfo> paymentMethodService)
			=> paymentMethodService.QueryAsync(new AllPaymentMethodsQuery())
				.Select(pm => new PaymentMethodViewModel
				{
					Id = pm.Id,
					Name = pm.Name,
					BankType = pm.BankType
				});
	}
}
