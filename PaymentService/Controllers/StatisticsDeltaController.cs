using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices.Statistics;
using Tgs.Payment.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Controllers;

[Route("statistics-delta")]
[ApiController]
public class StatisticsDeltaController : ControllerBase
{
	[HttpGet(Name = "QueryStatisticsDelta")]
	public IAsyncEnumerable<PaymentStatisticsDeltaViewModel> QueryStatisticsDeltaAsync(
		[FromServices] IQueryService<PaymentStatistics5minDeltaQuery, UserPaymentStatisticsDelta> queryService,
		[FromQuery] Guid anchorId)
	{
		var query = new PaymentStatistics5minDeltaQuery(anchorId);

		return queryService.QueryAsync(query, HttpContext.RequestAborted)
			.Select(item => new PaymentStatisticsDeltaViewModel
			{
				Id = item.Id,
				TimePoint = item.TimePoint,
				UserId = item.UserId,
				DepositAmount = item.DepositAmount,
				DepositCount = item.DepositCount,
				DepositHandlingFee = item.DepositHandlingFee,
				WithdrawAmount = item.WithdrawAmount,
				WithdrawCount = item.WithdrawCount,
				WithdrawHandlingFee = item.WithdrawHandlingFee
			});
	}
}
