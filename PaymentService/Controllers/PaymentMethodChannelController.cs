using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.Payment.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Controllers
{
	[Route("payment/pmc")]
	[ApiController]
	public class PaymentMethodChannelController : ControllerBase
	{
		[HttpGet]
		public IAsyncEnumerable<PaymentMethodChannelViewModel> GetAllPaymentMethodChannelsAsync(
			[FromServices] IQueryService<AllPaymentMethodChannelsQuery, PaymentMethodChannelInfo> queryService)
		{
			var allPayments = queryService.QueryAsync(new AllPaymentMethodChannelsQuery());

			return allPayments.Select(pmc => new PaymentMethodChannelViewModel
			{
				Id = pmc.Id,
				PaymentId = pmc.PaymentId,
				MethodId = pmc.MethodId,
				ChannelId = pmc.ChannelId
			});
		}
	}
}
