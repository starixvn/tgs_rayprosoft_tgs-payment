using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices.Statistics;
using Tgs.Payment.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Controllers;

[Route("[controller]")]
[ApiController]
public class StatisticsController : ControllerBase
{
	[HttpGet("{start}/{end}", Name = "QueryPaymentStatistics")]
	public IAsyncEnumerable<PaymentStatisticsViewModel> QueryPaymentStatisticsAsync(
		[FromServices] IQueryService<PaymentStatisticsQuery, UserPaymentSummaryStatistics> queryService,
		DateOnly start,
		DateOnly end,
		[FromQuery(Name = "userId")] string[]? userIds)
	{
		var query = new PaymentStatisticsQuery(
			start,
			end,
			userIds);

		return queryService.QueryAsync(query, HttpContext.RequestAborted)
			.Select(s => new PaymentStatisticsViewModel
			{
				TimePoint = s.TimePoint,
				UserId = s.UserId,
				DepositAmount = s.DepositAmount,
				DepositCount = s.DepositCount,
				DepositHandlingFee = s.DepositHandlingFee,
				WithdrawAmount = s.WithdrawAmount,
				WithdrawCount = s.WithdrawCount,
				WithdrawHandlingFee = s.WithdrawHandlingFee
			});
	}

	[HttpGet("5min/{start}/{end}", Name = "Query5MinPaymentStatistics")]
	public IAsyncEnumerable<PaymentStatisticsViewModel> Query5MinPaymentStatisticsAsync(
		[FromServices] IQueryService<PaymentStatistics5minQuery, UserPaymentSummaryStatistics> queryService,
		DateTime start,
		DateTime end)
	{
		var query = new PaymentStatistics5minQuery(start, end);

		return queryService.QueryAsync(query, HttpContext.RequestAborted)
			.Select(s => new PaymentStatisticsViewModel
			{
				TimePoint = s.TimePoint,
				UserId = s.UserId,
				DepositAmount = s.DepositAmount,
				DepositCount = s.DepositCount,
				DepositHandlingFee = s.DepositHandlingFee,
				WithdrawAmount = s.WithdrawAmount,
				WithdrawCount = s.WithdrawCount,
				WithdrawHandlingFee = s.WithdrawHandlingFee
			});
	}
}
