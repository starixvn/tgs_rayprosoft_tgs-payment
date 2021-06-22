using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices;
using Tgs.Payment.ViewModels;
using TGS.ServiceUtility;

namespace Tgs.Payment.Controllers
{
	[Route("payment/channel")]
	[ApiController]
	public class PaymentChannelController : ControllerBase
	{
		[HttpGet]
		public IAsyncEnumerable<PaymentChannelViewModel> GetAllPaymentChennelsAsync(
			[FromServices] IQueryService<AllPaymentChannelsQuery, PaymentChannelInfo> paymentChannelService)
			=> paymentChannelService.QueryAsync(new AllPaymentChannelsQuery())
				.Select(pc => new PaymentChannelViewModel
				{
					Id = pc.Id,
					Name = pc.Name
				});
	}
}
