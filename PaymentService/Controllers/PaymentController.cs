using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices;
using Tgs.Payment.ViewModels;
using TGS.ServiceUtility;

namespace Tgs.Payment.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class PaymentController : ControllerBase
	{
		[HttpGet("{id}")]
		public async Task<PaymentViewModel> GetPaymentByIdAsync(
			[FromServices] IGetService<GetPaymentQuery, PaymentInfo> getService,
			Guid id)
		{
			var query = new GetPaymentQuery(id);

			var info = await getService.GetAsync(query).ConfigureAwait(false);

			return new PaymentViewModel
			{
				Id = info.Id,
				Code = info.Code
			};
		}

		[HttpGet]
		public IAsyncEnumerable<PaymentViewModel> GetAllPaymentsAsync(
			[FromServices] IQueryService<GetAllPaymentsQuery, PaymentInfo> queryService)
		{
			var allPayments = queryService.QueryAsync(new GetAllPaymentsQuery());

			return allPayments.Select(payment => new PaymentViewModel
			{
				Id = payment.Id,
				Code = payment.Code
			});
		}
	}
}
