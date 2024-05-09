using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.Payment.Repositories.Entities;

namespace Tgs.Payment.Repositories;

internal class StatisticsRepository(TgsContext dbContext)
	: IStatisticsRepository
{
	public IAsyncEnumerable<UserPaymentSummaryStatistics> Query5minPaymentStatisticsAsync(
		DateTime start,
		DateTime end,
		string[]? userIds,
		CancellationToken cancellationToken = default)
	{
		var query = dbContext.FiveMinutesPaymentStatistics
			.Where(row => row.TimePoint >= start.ToUniversalTime()
				&& row.TimePoint < end.ToUniversalTime()
				&& row.UserType == (int)UserType.Player);

		if (userIds is not null && userIds.Length > 0)
			query = query.Where(row => userIds.Contains(row.UserId));

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
		string[]? userIds,
		CancellationToken cancellationToken = default)
	{
		var query = dbContext.HourPaymentStatistics
			.Where(row => row.TimePoint >= start.ToUniversalTime()
				&& row.TimePoint < end.ToUniversalTime()
				&& row.UserType == (int)UserType.Player);

		if (userIds is not null && userIds.Length > 0)
			query = query.Where(row => userIds.Contains(row.UserId));

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
		string[]? userIds,
		CancellationToken cancellationToken = default)
	{
		var query = dbContext.PlayerDailyPaymentStatistics
			.Where(row => row.ReportDate >= start
				&& row.ReportDate < end);

		if (userIds is not null && userIds.Length > 0)
			query = query.Where(row => userIds.Contains(row.PlayerCode));

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

	public IAsyncEnumerable<UserPaymentStatisticsDelta> Query5minPaymentStatisticsDeltaAsync(
		Guid anchorId,
		Guid stopId,
		CancellationToken cancellationToken = default)
		=> dbContext.FiveMinutesPaymentStatisticsDelta
			.Where(row => row.Id > anchorId
				&& row.Id < stopId
				&& row.UserType == (int)UserType.Player)
			.Take(1000)
			.ToAsyncEnumerable()
			.Select(row => new UserPaymentStatisticsDelta(
				row.Id,
				row.TimePoint,
				row.UserId,
				UserType.Player,
				row.DepositAmount,
				row.DepositHandlingFee,
				row.DepositCount,
				row.WithdrawAmount,
				row.WithdrawHandlingFee,
				row.WithdrawCount));

	public async ValueTask<decimal> QueryBrandNetDepositAsync(
		DateTime start,
		DateTime end,
		Guid brandId,
		CancellationToken cancellationToken = default)
	{
		var query = await dbContext.FiveMinutesPaymentStatistics
			.Where(row => row.TimePoint >= start.ToUniversalTime()
				&& row.TimePoint < end.ToUniversalTime()
				&& row.UserType == (int)UserType.Player
				&& row.BrandId == brandId)
			.SumAsync(row => row.DepositAmount.GetValueOrDefault(0m) - row.WithdrawAmount.GetValueOrDefault(0m), cancellationToken)
			.ConfigureAwait(false);

		return query;
	}
}