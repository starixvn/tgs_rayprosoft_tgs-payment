using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices.Players;
using Tgs.Payment.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Controllers;

[Route("[controller]")]
[ApiController]
public class PlayerController : ControllerBase
{
	[HttpGet("overview/{playerCode}", Name = "GetPlayerPaymentOverview")]
	public async Task<PaymentOverviewViewModel> GetPlayerPaymentOverViewAsync(
		[FromServices] IGetService<GetPaymentOverviewQuery, PaymentOverview?> getService,
		string playerCode)
	{
		var query = new GetPaymentOverviewQuery(playerCode);

		var result = await getService.GetAsync(
			query,
			HttpContext.RequestAborted).ConfigureAwait(false)
			?? PaymentOverview.Default;

		return new PaymentOverviewViewModel
		{
			FirstDepositAmount = result.FirstDepositAmount,
			FirstDepositHandlingFee = result.FirstDepositHandlingFee,
			FirstDepositOn = result.FirstDepositOn,
			FirstWithdrawAmount = result.FirstWithdrawAmount,
			FirstWithdrawHandlingFee = result.FirstWithdrawHandlingFee,
			FirstWithdrawOn = result.FirstWithdrawOn,
			TotalDepositAmount = result.TotalDepositAmount,
			TotalWithdrawAmount = result.TotalWithdrawAmount,
			RecordedBy = result.RecordedBy
		};
	}
}
