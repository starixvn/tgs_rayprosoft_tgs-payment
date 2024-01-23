using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tgs.Payment.Repositories.Entities;

namespace Tgs.Payment.Repositories;

internal class StatisticsRepository(TgsContext dbContext)
	: IStatisticsRepository
{
	public IAsyncEnumerable<UserPaymentSummaryStatistics> Query5minPaymentStatisticsAsync(
		DateTime start,
		DateTime end,
		string? userId,
		CancellationToken cancellationToken = default)
	{
		var query = dbContext.FiveMinutesPaymentStatistics
			.Where(row => row.TimePoint >= start.ToUniversalTime()
				&& row.TimePoint <= end.ToUniversalTime()
				&& row.UserType == (int)UserType.Player);

		if (userId is not null)
			query = query.Where(row => row.UserId == userId);

		return query.OrderBy(row => row.TimePoint)
			.ToAsyncEnumerable()
			.Select(row => new UserPaymentSummaryStatistics(
				row.TimePoint,
				row.UserId,
				(UserType)row.UserType,
				row.DepositAmount,
				row.DepositHandlingFee,
				row.DepositCount,
				row.FirstDepositAmount,
				row.FirstDepositHandlingFee,
				row.FirstDepositOn,
				row.WithdrawAmount,
				row.WithdrawHandlingFee,
				row.WithdrawCount,
				row.FirstWithdrawAmount,
				row.FirstWithdrawHandlingFee,
				row.FirstWithdrawOn));
	}

	public IAsyncEnumerable<UserPaymentSummaryStatistics> QueryHourPaymentStatisticsAsync(
		DateTime start,
		DateTime end,
		string? userId,
		CancellationToken cancellationToken = default)
	{
		var query = dbContext.HourPaymentStatistics
			.Where(row => row.TimePoint >= start.ToUniversalTime()
				&& row.TimePoint <= end.ToUniversalTime()
				&& row.UserType == (int)UserType.Player);

		if (userId is not null)
			query = query.Where(row => row.UserId == userId);

		return query.OrderBy(row => row.TimePoint)
			.ToAsyncEnumerable()
			.Select(row => new UserPaymentSummaryStatistics(
				row.TimePoint,
				row.UserId,
				(UserType)row.UserType,
				row.DepositAmount,
				row.DepositHandlingFee,
				row.DepositCount,
				row.FirstDepositAmount,
				row.FirstDepositHandlingFee,
				row.FirstDepositOn,
				row.WithdrawAmount,
				row.WithdrawHandlingFee,
				row.WithdrawCount,
				row.FirstWithdrawAmount,
				row.FirstWithdrawHandlingFee,
				row.FirstWithdrawOn));
	}

	public IAsyncEnumerable<UserPaymentSummaryStatistics> QueryDailyPaymentStatisticsAsync(
		DateOnly start,
		DateOnly end,
		string? userId,
		CancellationToken cancellationToken = default)
	{
		var query = dbContext.PlayerDailyPaymentStatistics
			.Where(row => row.ReportDate >= start
				&& row.ReportDate <= end);

		if (userId is not null)
			query = query.Where(row => row.PlayerCode == userId);

		return query.OrderBy(row => row.ReportDate)
			.ToAsyncEnumerable()
			.Select(row => new UserPaymentSummaryStatistics(
				new DateTimeOffset(row.ReportDate, TimeOnly.MinValue, TimeSpan.FromHours(8D)).UtcDateTime,
				row.PlayerCode,
				UserType.Player,
				row.DailyDepositAmount,
				row.DailyDepositHandlingFee,
				row.DailyDepositCount,
				row.FirstDepositAmount,
				row.FirstDepositHandlingFee,
				row.FirstDepositOn,
				row.DailyWithdrawAmount,
				row.DailyWithdrawHandlingFee,
				row.DailyWithdrawCount,
				row.FirstWithdrawAmount,
				row.FirstWithdrawHandlingFee,
				row.FirstWithdrawOn));
	}
}
