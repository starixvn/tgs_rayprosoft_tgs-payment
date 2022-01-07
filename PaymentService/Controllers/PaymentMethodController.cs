using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.Payment.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Controllers;

[Route("payment/method")]
[ApiController]
public class PaymentMethodController : ControllerBase
{
	[HttpPut(Name = "CreatePaymentMethod")]
	public ValueTask CreatePaymentMethodAsync(
		[FromServices] ICommandService<CreatePaymentMethodCommand> createService,
		[FromBody] PaymentMethodCreateViewModel request)
	{
		var command = new CreatePaymentMethodCommand(
			request.Code,
			request.BankType);

		return createService.ExecuteAsync(command);
	}

	[HttpGet(Name = "GetAllPaymentMethods")]
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
