using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices.Players;
using Tgs.Payment.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Controllers;

[Route("[controller]")]
[ApiController]
public class PlayerController : ControllerBase
{
	[HttpGet("total-amount/current-day", Name = "GetCurrentDayDepositWithdrawTotalAmount")]
	public async IAsyncEnumerable<TopTenDepositWithdrawViewModel> GetCurrentDayDepositWithdrawTotalAmountAsync(
		[FromServices] IQueryService<GetCurrentDayTotalAmountQuery, TopTenDepositWithdrawInfo> queryService,
		[FromQuery(Name = "player")] IEnumerable<string> playerCodes)
	{
		var query = new GetCurrentDayTotalAmountQuery(playerCodes);

		var result = await queryService
			.QueryAsync(query, HttpContext.RequestAborted)
			.ToArrayAsync(HttpContext.RequestAborted)
			.ConfigureAwait(false);

		foreach (var item in result)
		{
			yield return new TopTenDepositWithdrawViewModel
			{
				PlayerCode = item.PlayerCode,
				DepositTotalAmount = item.DepositTotalAmount,
				WithdrawTotalAmount = item.WithdrawTotalAmount,
			};
		}
	}

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
