using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.Payment.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Controllers;

[Route("payment/channel")]
[ApiController]
public class PaymentChannelController : ControllerBase
{
	[HttpPut(Name = "CreatePaymentChannel")]
	public ValueTask CreatePaymentChannelAsync(
		[FromServices] ICommandService<CreatePaymentChannelCommand> createService,
		[FromBody] string code)
		=> createService.ExecuteAsync(new CreatePaymentChannelCommand(code));

	[HttpGet(Name = "GetAllPaymentChannels")]
	public IAsyncEnumerable<PaymentChannelViewModel> GetAllPaymentChennelsAsync(
		[FromServices] IQueryService<AllPaymentChannelsQuery, PaymentChannelInfo> paymentChannelService)
		=> paymentChannelService.QueryAsync(new AllPaymentChannelsQuery())
			.Select(pc => new PaymentChannelViewModel
			{
				Id = pc.Id,
				Name = pc.Name
			});
}
