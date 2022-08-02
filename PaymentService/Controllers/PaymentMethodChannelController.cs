using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.Payment.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Controllers;

[Route("payment/pmc")]
[ApiController]
public class PaymentMethodChannelController : ControllerBase
{
	[HttpPut(Name = "CreatePaymentMethodChannel")]
	public ValueTask<Guid> CreatePaymentMethodChannelAsync(
		[FromServices] IGetService<CreatePaymentMethodChannelCommand, Guid> createService,
		[FromBody] CreatePaymentMethodChannelViewModel viewModel)
	{
		var command = new CreatePaymentMethodChannelCommand(
			viewModel.PaymentId,
			viewModel.MethodId,
			viewModel.ChannelId);

		return createService.GetAsync(command, HttpContext.RequestAborted);
	}

	[HttpGet(Name = "GetAllPaymentMethodChannels")]
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
