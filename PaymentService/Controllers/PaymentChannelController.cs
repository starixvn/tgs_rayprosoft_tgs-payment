using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices;
using TGS.ServiceUtility;

namespace Tgs.Payment.Controllers
{
	[Route("payment/channel")]
	[ApiController]
	public class PaymentChannelController : ControllerBase
	{
		public IAsyncEnumerable<PaymentChannelInfo> GetAllPaymentChennelsAsync(
			[FromServices] IQueryService<AllPaymentChannelsQuery, PaymentChannelInfo> paymentChannelService)
			=> paymentChannelService.QueryAsync(new AllPaymentChannelsQuery());
	}
}
