using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Tgs.Paging;
using Tgs.Payment;
using Tgs.Payment.Repositories;
using Tgs.Payment.Repositories.Entities;

namespace TGS.Payment.Repositories;

internal class PaymentRepository(TgsContext tgsContext) : IPaymentRepository
{
	public async ValueTask<decimal> CalculateDepositTotalAmountAsync(DepositFormQueryConditions conditions, CancellationToken cancellationToken = default)
	{
		var dataQuery = MakeDepositFormQuery(conditions);

		return await dataQuery
			.SumAsync(x => x.ActualAmount ?? x.Amount, cancellationToken)
			.ConfigureAwait(false);
	}

	public async ValueTask<decimal> CalculateWithdrawTotalAmountAsync(WithdrawFormQueryConditions conditions, CancellationToken cancellationToken = default)
	{
		var dataQuery = MakeWithdrawFormQueryWithLimit90Days(conditions);

		return await dataQuery
			.SumAsync(x => x.ActualAmount ?? x.Amount, cancellationToken)
			.ConfigureAwait(false);
	}

	public async ValueTask<bool> CheckPlayerBankInfoIsExistsAsync(
		Guid brandId,
		string bankType,
		Guid channelId,
		byte[] hash,
		string playerCode,
		CancellationToken cancellationToken = default)
		=> await tgsContext.PlayerBankInfos
			.Where(info => info.BankInfoHash == hash
				&& info.BrandId == brandId
				&& info.BankType == bankType
				&& info.PaymentChannelId == channelId)
			.AsNoTracking()
			.AnyAsync(cancellationToken)
			.ConfigureAwait(false);

	public async ValueTask<DepositFormsStatusCount> CountCurrentDayDepositFormsStatusAsync(Guid brandId, CancellationToken cancellationToken = default)
	{
		var currentDay = new DateTimeOffset(
			DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8D)).Date,
			TimeSpan.FromHours(8D))
			.UtcDateTime;

		var result = await tgsContext.Set<DepositStatusSummary>()
			.FromSqlInterpolated($"""
				SELECT
					count(1) as total,
					count(1) filter(where status=8000) as approved,
					count(1) filter(where status=7000) as reject
				FROM dbo.deposit_forms
				WHERE
					brand_id = {brandId}
					AND
					updated_on >= {currentDay}
				""")
			.AsNoTracking()
			.FirstOrDefaultAsync(cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		return new DepositFormsStatusCount { Approved = result!.Approved, Reject = result.Reject, Total = result.Total };
	}

	public async ValueTask<int> CountDepositFormsAsync(DepositFormQueryConditions conditions, CancellationToken cancellationToken = default)
	{
		var dataQuery = MakeDepositFormQuery(conditions);

		return await dataQuery
			.CountAsync(cancellationToken)
			.ConfigureAwait(false);
	}

	public async ValueTask<int> CountWithdrawFormsAsync(WithdrawFormQueryConditions conditions, CancellationToken cancellationToken = default)
	{
		var dataQuery = MakeWithdrawFormQueryWithLimit90Days(conditions);

		return await dataQuery
			.CountAsync(cancellationToken)
			.ConfigureAwait(false);
	}

	public async ValueTask<DepositFormsStatusCount> CountYesterDayDepositFormsStatusAsync(Guid brandId, CancellationToken cancellationToken = default)
	{
		var currentDay = new DateTimeOffset(
			DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8D)).Date,
			TimeSpan.FromHours(8D))
			.UtcDateTime;

		var yesterday = currentDay.AddDays(-1);

		var result = await tgsContext.Set<DepositStatusSummary>()
			.FromSqlInterpolated($"""
				SELECT
					count(1) as total,
					count(1) filter(where status=8000) as approved,
					count(1) filter(where status=7000) as reject
				FROM dbo.deposit_forms
				WHERE
					brand_id = {brandId}
					AND
					updated_on >= {yesterday}
					AND
					updated_on < {currentDay}
				""")
			.AsNoTracking()
			.FirstOrDefaultAsync(cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		return new DepositFormsStatusCount { Approved = result!.Approved, Reject = result.Reject, Total = result.Total };
	}

	public async ValueTask CreateAgentBankInfoAsync(AgentBankInfo agentBankInfo, string modifyUser, CancellationToken cancellationToken = default)
	{
		var createTime = DateTimeOffset.UtcNow;

		_ = await tgsContext.AgentBankInfoItems
			.AddAsync(new AgentBankInfoItem
			{
				Id = agentBankInfo.Id,
				AgentId = agentBankInfo.AgentId,
				BrandId = agentBankInfo.BrandId,
				PaymentChannelId = agentBankInfo.PaymentChannelId,
				BankType = agentBankInfo.BankType,
				Detail = agentBankInfo.Detail,
				CreatedByUsername = modifyUser,
				CreatedOn = createTime.UtcDateTime,
				UpdatedByUsername = modifyUser,
				UpdatedOn = createTime.UtcDateTime
			}, cancellationToken).ConfigureAwait(false);

		_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask CreateBankInfoAsync(Tgs.Payment.PlayerBankCreateInfo bankInfo, string modifyUser, CancellationToken cancellationToken = default)
	{
		var createTime = DateTime.UtcNow;

		_ = await tgsContext.PlayerBankInfos
			.AddAsync(new Tgs.Payment.Repositories.Entities.PlayerBankInfo
			{
				Id = bankInfo.Id,
				PlayerCode = bankInfo.PlayerCode,
				BrandId = bankInfo.BrandId,
				PaymentChannelId = bankInfo.PaymentChannelId,
				BankType = bankInfo.BankType,
				Detail = JsonSerializer.Serialize(bankInfo.Detail),
				Default = bankInfo.IsDefault,
				BankInfoHash = bankInfo.DetailHash,
				CreatedByUsername = modifyUser,
				CreatedOn = createTime,
				UpdatedByUsername = modifyUser,
				UpdatedOn = createTime
			}, cancellationToken)
			.ConfigureAwait(false);

		_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask CreateBrandAutoWithdrawEventAsync(
		Guid eventId,
		Guid brandId,
		JsonDocument changeSets,
		string modifier,
		CancellationToken cancellationToken = default)
	{
		_ = await tgsContext.BrandPaymentAutoWithdrawEventSources.AddAsync(
			new BrandPaymentAutoWithdrawEventSource
			{
				Id = eventId,
				BrandId = brandId,
				ChangeSets = JsonSerializer.Serialize(changeSets),
				ModifyUser = modifier,
				CreatedOn = DateTime.UtcNow
			},
			cancellationToken).ConfigureAwait(false);

		_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask CreateDepositFormEventAsync(
			Guid eventId,
		Guid formId,
		EventAction action,
		JsonDocument changeSets,
		string? modifier,
		CancellationToken cancellationToken = default)
	{
		_ = await tgsContext.DepositFormEventSources.AddAsync(
			new DepositFormEventSource
			{
				Id = eventId,
				FormId = formId,
				Action = action.ToString(),
				ChangeSets = JsonSerializer.Serialize(changeSets),
				ModifyUser = modifier,
				CreatedOn = DateTime.UtcNow
			},
			cancellationToken).ConfigureAwait(false);

		_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask CreatePaymentAsync(PaymentCreateInfo createInfo, CancellationToken cancellationToken = default)
	{
		var createPayment = new PaymentItem
		{
			Id = Guid.NewGuid(),
			Code = createInfo.Code,
			Name = createInfo.Code,
			Description = createInfo.Description,
			ExtendData = createInfo.ExtendedData,
			CreatedByUsername = "admin.enabled",
			UpdatedByUsername = "admin.enabled"
		};

		_ = await tgsContext.PaymentItems
			.AddAsync(createPayment, cancellationToken)
			.ConfigureAwait(false);

		_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask CreatePaymentChannelAsync(string channelCode, CancellationToken cancellationToken = default)
	{
		var createChannel = new PaymentChannel
		{
			Id = Guid.NewGuid(),
			Code = channelCode
		};

		_ = await tgsContext.PaymentChannels
			.AddAsync(createChannel, cancellationToken)
			.ConfigureAwait(false);

		_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask CreatePaymentMethodAsync(string methodCode, string bankType, CancellationToken cancellationToken = default)
	{
		var createMethod = new PaymentMethod
		{
			Id = Guid.NewGuid(),
			Code = methodCode,
			BankType = bankType
		};

		_ = await tgsContext.PaymentMethods
			.AddAsync(createMethod, cancellationToken)
			.ConfigureAwait(false);

		_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask<Guid> CreatePaymentMethodChannelAsync(Guid paymentId, Guid methodId, Guid channelId, CancellationToken cancellationToken = default)
	{
		var pmc = new PaymentMethodChannel
		{
			Id = Guid.NewGuid(),
			PaymentId = paymentId,
			MethodId = methodId,
			ChannelId = channelId
		};

		_ = await tgsContext.PaymentMethodChannels
			.AddAsync(pmc, cancellationToken)
			.ConfigureAwait(false);

		_ = await tgsContext
			.SaveChangesAsync(cancellationToken)
			.ConfigureAwait(false);

		return pmc.Id;
	}

	public async ValueTask CreateWithdrawFormEventAsync(
		Guid eventId,
		Guid formId,
		EventAction action,
		JsonDocument changeSets,
		string? modifier,
		CancellationToken cancellationToken = default)
	{
		_ = await tgsContext.WithdrawFormEventSources.AddAsync(
			new WithdrawFormEventSource
			{
				Id = eventId,
				FormId = formId,
				Action = action.ToString(),
				ChangeSets = JsonSerializer.Serialize(changeSets),
				ModifyUser = modifier,
				CreatedOn = DateTime.UtcNow
			},
			cancellationToken).ConfigureAwait(false);

		_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask DeleteAgentBankInfoAsync(Guid agentId, Guid bankInfoId, CancellationToken cancellationToken = default)
	{
		var bankInfo = await tgsContext.AgentBankInfoItems
			.FindAsync([bankInfoId], cancellationToken)
			.ConfigureAwait(false);

		if (bankInfo != null && bankInfo.AgentId == agentId)
		{
			_ = tgsContext.AgentBankInfoItems.Remove(bankInfo);

			_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	public async ValueTask DeleteBankInfoAsync(string playerCode, Guid bankInfoId, CancellationToken cancellationToken = default)
	{
		var bankInfo = await tgsContext.PlayerBankInfos
			.FindAsync([bankInfoId], cancellationToken)
			.ConfigureAwait(false);

		if (bankInfo != null && bankInfo.PlayerCode == playerCode)
		{
			_ = tgsContext.PlayerBankInfos.Remove(bankInfo);

			_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	public async ValueTask DeletePaymentMethodChannelAsync(Guid pmcId, CancellationToken cancellationToken = default)
	{
		var deletePmc = new PaymentMethodChannel
		{
			Id = pmcId
		};

		tgsContext.Entry(deletePmc).State = EntityState.Deleted;

		_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask<AgentBankInfo> GetAgentBankInfoByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var bankInfo = await tgsContext.AgentBankInfoItems
			.FindAsync([id], cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		return new AgentBankInfo(
			bankInfo!.Id,
			bankInfo.AgentId,
			bankInfo.BrandId,
			bankInfo.PaymentChannelId,
			bankInfo.BankType,
			bankInfo.Detail!);
	}

	public IAsyncEnumerable<AgentBankInfo> GetAgentBankInfosByAgentIdAsync(Guid agentId, CancellationToken cancellationToken = default)
		=> tgsContext.AgentBankInfoItems.AsQueryable()
			.Where(info => info.AgentId == agentId)
			.Select(info => new AgentBankInfo(
				info.Id,
				info.AgentId,
				info.BrandId,
				info.PaymentChannelId,
				info.BankType,
				info.Detail!))
			.ToAsyncEnumerable();

	public async ValueTask<AgentInfo> GetAgentInfoByAgentIdAsync(Guid agentId, CancellationToken cancellationToken = default)
	{
		var agentInfo = await tgsContext.Agents
			.FindAsync([agentId], cancellationToken)
			.ConfigureAwait(false);

		return new AgentInfo(agentInfo!.Phone);
	}

	public IAsyncEnumerable<PaymentChannelInfo> GetAllPaymentChannelsAsync(CancellationToken cancellationToken = default)
		=> tgsContext.PaymentChannels.AsQueryable()
			.Select(row => new PaymentChannelInfo(row.Id, row.Code))
			.ToAsyncEnumerable();

	public IAsyncEnumerable<PaymentMethodChannelInfo> GetAllPaymentMethodChannelsAsync(CancellationToken cancellationToken = default)
		=> tgsContext.PaymentMethodChannels.AsQueryable()
			.Select(row => new PaymentMethodChannelInfo(
				row.Id,
				row.PaymentId,
				row.MethodId,
				row.ChannelId))
			.ToAsyncEnumerable();

	public IAsyncEnumerable<PaymentMethodInfo> GetAllPaymentMethodsAsync(CancellationToken cancellationToken = default)
		=> tgsContext.PaymentMethods.AsQueryable()
			.Select(row => new PaymentMethodInfo(row.Id, row.Code, row.BankType))
			.ToAsyncEnumerable();

	public IAsyncEnumerable<PaymentInfo> GetAllPaymentsAsync(CancellationToken cancellationToken = default)
		=> tgsContext.PaymentItems.AsQueryable()
			.Select(row => new PaymentInfo(row.Id, row.Code))
			.ToAsyncEnumerable();

	public async ValueTask<Tgs.Payment.PlayerBankInfo?> GetBankInfoByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var bankInfo = await tgsContext.PlayerBankInfos
			.FindAsync([id], cancellationToken)
			.ConfigureAwait(false);

		return bankInfo is null
			? null
			: new Tgs.Payment.PlayerBankInfo(
				bankInfo!.Id,
				bankInfo.PlayerCode,
				bankInfo.BrandId,
				bankInfo.PaymentChannelId,
				bankInfo.BankType,
				string.IsNullOrEmpty(bankInfo.Detail) ? JsonDocument.Parse("{}") : JsonDocument.Parse(bankInfo.Detail),
				bankInfo.Default,
				DateTime.UtcNow,
				null);
	}

	public IAsyncEnumerable<Tgs.Payment.PlayerBankInfo> GetBankInfosByPlayerCodeAsync(string playerCode, CancellationToken cancellationToken = default)
		=> tgsContext.PlayerBankInfos.AsQueryable()
			.Where(info => info.PlayerCode == playerCode)
			.Select(info => new
			{
				info.Id,
				info.PlayerCode,
				info.BrandId,
				info.PaymentChannelId,
				info.BankType,
				info.Detail,
				info.Default,
				info.UpdatedOn,
				info.UpdatedByUsername
			})
			.ToAsyncEnumerable()
			.Select(info => new Tgs.Payment.PlayerBankInfo(
				info.Id,
				info.PlayerCode,
				info.BrandId,
				info.PaymentChannelId,
				info.BankType,
				string.IsNullOrEmpty(info.Detail) ? JsonDocument.Parse("{}") : JsonDocument.Parse(info.Detail),
				info.Default,
				info.UpdatedOn,
				info.UpdatedByUsername));

	public async ValueTask<AutoWithdrawInfo?> GetBrandAutoWithdrawSettingAsync(Guid brandId, CancellationToken cancellationToken = default)
	{
		var autoWithdrawSetting = await tgsContext.BrandPaymentAutoWithdraws
			.FindAsync([brandId], cancellationToken)
			.ConfigureAwait(false);

		if (autoWithdrawSetting is null)
			return null;

		IEnumerable<Guid> brandPaymentIds;

		var jsonNode = JsonNode.Parse(autoWithdrawSetting.BrandPaymentCandidates);

		brandPaymentIds = jsonNode is null
			? []
			: jsonNode!["brandPaymentIds"]?.AsArray().Select(x => x!.GetValue<Guid>()).AsEnumerable()
				?? [];

		return new AutoWithdrawInfo(
				autoWithdrawSetting.EnableAutoWithdraw,
				autoWithdrawSetting.AmountUpperLimit,
				brandPaymentIds);
	}

	public async ValueTask<bool> GetBrandCheckPlayerBankInfoDuplicateIsEnabledAsync(Guid brandId, CancellationToken cancellationToken = default)
	{
		var flag = await tgsContext.BrandPlayerBankinfoCheckFlags.FindAsync(
			[brandId],
			cancellationToken: cancellationToken).ConfigureAwait(false);

		return flag is not null && flag.IsEnabled;
	}

	public async ValueTask<decimal?> GetBrandNetDepositAsync(Guid brandId, DateOnly date, CancellationToken cancellationToken = default)
	{
		var netprofit = await tgsContext.BrandDailyPaymentStatistics
			.FirstOrDefaultAsync(p => p.BrandId == brandId && p.ReportDate == date, cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		return netprofit == null
			? null
			: netprofit.DailyDepositAmount - netprofit.DailyWithdrawAmount;
	}

	public IAsyncEnumerable<BrandPaymentAcl> GetBrandPaymentAclsAsync(IEnumerable<Guid> brandPaymentIds, string playerCode, CancellationToken cancellationToken = default)
	{
		var ids = brandPaymentIds.ToArray();

		var query = from acl in tgsContext.BrandPaymentPlayers.AsQueryable()
					where acl.PlayerCode == playerCode
						&& ids.Contains(acl.BrandPaymentId)
					select acl;

		return query.Select(acl => new BrandPaymentAcl(
			acl.BrandPaymentId,
			acl.PlayerCode,
			acl.ListType == 1)).ToAsyncEnumerable();
	}

	public async ValueTask<BrandPaymentInfo> GetBrandPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var row = await tgsContext.BrandPayments.AsQueryable()
			.Include(bp => bp.PaymentMethodChannel)
			.ThenInclude(pmc => pmc.Channel)
			.Where(bp => bp.Id == id)
			.SingleAsync(cancellationToken)
			.ConfigureAwait(false);

		return new BrandPaymentInfo(
			row.Id,
			row.PaymentMethodChannelId,
			row.PaymentMethodChannel.PaymentId,
			row.PaymentMethodChannel.MethodId,
			row.PaymentMethodChannel.ChannelId,
			row.ChannelName,
			row.PaymentMethodChannel.Channel.Code,
			row.ExtendedData,
			new ValueRange<decimal>
			{
				From = row.TransferLimitFrom,
				To = row.TransferLimitTo
			},
			row.AllPlayers,
			row.IdentityAuthentication,
			row.PhoneAuthentication,
			new ValueRange<decimal>
			{
				From = row.DepositedAmountMin,
				To = row.DepositedAmountUnlimited ? decimal.MaxValue : row.DepositedAmountMax
			},
			row.PlayerListCondition == 1 ? ConditionMatchType.MatchOne : ConditionMatchType.MatchAll,
			row.TransferType switch
			{
				1 => TransferType.Withdraw,
				_ => TransferType.Deposit,
			},
			row.PaymentStatus == 0,
			row.DisplayOrder,
			row.BrandId);
	}

	public IAsyncEnumerable<BrandPaymentPlayerInfo> GetBrandPaymentDesignatedUsersAsync(
		Guid brandPaymentId,
		CancellationToken cancellationToken = default)
	{
		var brandPayment = tgsContext.BrandPayments.AsQueryable()
					.Where(bp => bp.Id == brandPaymentId)
					.Include(bp => bp.BrandPaymentPlayers)
					.ThenInclude(bp => bp.PlayerCodeNavigation)
					.Include(bp => bp.BrandPaymentVipLevels)
					.AsNoTracking()
					.FirstOrDefault();

		var vipLevelIds = brandPayment!.BrandPaymentVipLevels.Select(bpvl => bpvl.VipLevelId).ToArray();

		var playerListType = (short)PlayerListType.White;
		var playerCodes = brandPayment.BrandPaymentPlayers
			.Where(bpp => bpp.ListType == playerListType)
			.Select(bpp => bpp.PlayerCode)
			.ToArray();

		var isAll = brandPayment.PlayerListCondition == (int)ConditionMatchType.MatchAll;

		var query = brandPayment.BrandPaymentPlayers
			.Where(bpp => playerCodes.Contains(bpp.PlayerCode))
			.Where(bpp => bpp.PlayerCodeNavigation.AccumulatedDeposit >= brandPayment.DepositedAmountMin
				&& (brandPayment.DepositedAmountUnlimited || bpp.PlayerCodeNavigation.AccumulatedDeposit < brandPayment.DepositedAmountMax));

		if (isAll && brandPayment.IdentityAuthentication)
			query = query.Where(bpp => bpp.PlayerCodeNavigation.IdentityAuthentication);

		if (isAll && brandPayment.PhoneAuthentication)
			query = query.Where(bpp => bpp.PlayerCodeNavigation.PhoneAuthentication);

		if (isAll && brandPayment.BrandPaymentVipLevels.Count != 0)
			query = query.Where(bpp => bpp.PlayerCodeNavigation.VipId != null && vipLevelIds.Contains(bpp.PlayerCodeNavigation.VipId.Value));

		return query.Select(bpp => new BrandPaymentPlayerInfo(bpp.PlayerCodeNavigation.Username))
			.ToAsyncEnumerable();
	}

	public async ValueTask<BrandPaymentFeeInfo> GetBrandPaymentFeeSettingAsync(Guid brandPaymentId, CancellationToken cancellationToken = default)
	{
		var row = await tgsContext.BrandPayments.AsQueryable()
			.Where(bp => bp.Id == brandPaymentId)
			.SingleAsync(cancellationToken)
			.ConfigureAwait(false);

		return new BrandPaymentFeeInfo(
			row.Id,
			row.HandlingFeeRateTenPercentile,
			row.ReducePlayerAmount);
	}

	public IAsyncEnumerable<BrandPaymentFeeInfo> GetBrandPaymentFeeSettingsAsync(IEnumerable<Guid> brandPaymentIds, CancellationToken cancellationToken = default)
	{
		var ids = brandPaymentIds.ToArray();

		return tgsContext.BrandPayments.AsQueryable()
			.Where(bp => ids.Contains(bp.Id))
			.Select(bp => new BrandPaymentFeeInfo(bp.Id, bp.HandlingFeeRateTenPercentile, bp.ReducePlayerAmount))
			.ToAsyncEnumerable();
	}

	public IAsyncEnumerable<BrandPaymentInfo> GetBrandPaymentsAsync(Guid brandId, CancellationToken cancellationToken = default)
	{
		var query = from bp in tgsContext.BrandPayments.AsQueryable()
						.Include(bp => bp.PaymentMethodChannel)
						.ThenInclude(pmc => pmc.Channel)
						.AsAsyncEnumerable()
					where bp.BrandId == brandId
					select bp;

		return query.Select(bp => new BrandPaymentInfo(
			bp.Id,
			bp.PaymentMethodChannelId,
			bp.PaymentMethodChannel.PaymentId,
			bp.PaymentMethodChannel.MethodId,
			bp.PaymentMethodChannel.ChannelId,
			bp.ChannelName,
			bp.PaymentMethodChannel.Channel.Code,
			bp.ExtendedData,
			new ValueRange<decimal>
			{
				From = bp.TransferLimitFrom,
				To = bp.TransferLimitTo
			},
			bp.AllPlayers,
			bp.IdentityAuthentication,
			bp.PhoneAuthentication,
			new ValueRange<decimal>
			{
				From = bp.DepositedAmountMin,
				To = bp.DepositedAmountUnlimited ? decimal.MaxValue : bp.DepositedAmountMax
			},
			bp.PlayerListCondition == 1 ? ConditionMatchType.MatchOne : ConditionMatchType.MatchAll,
			bp.TransferType switch
			{
				1 => TransferType.Withdraw,
				_ => TransferType.Deposit
			},
			bp.PaymentStatus == 0,
			bp.DisplayOrder,
			bp.BrandId));
	}

	public async ValueTask<BrandPaymentSettings?> GetBrandPaymentSettingsAsync(Guid brandId, CancellationToken cancellationToken = default)
	{
		var row = await tgsContext.BrandPaymentSettings.FindAsync(
			[brandId],
			cancellationToken: cancellationToken).ConfigureAwait(false);

		return row is null
			? null
			: new BrandPaymentSettings(row.PlayerBankInfoCountLimit);
	}

	public IAsyncEnumerable<BrandPaymentVip> GetBrandPaymentVipListAsync(IEnumerable<Guid> brandPaymentIds, CancellationToken cancellationToken = default)
	{
		var ids = brandPaymentIds.ToArray();

		return tgsContext.BrandPaymentVipLevels.AsQueryable()
			.Where(v => ids.Contains(v.BrandPaymentId))
			.Select(v => new BrandPaymentVip(v.BrandPaymentId, v.VipLevelId))
			.ToAsyncEnumerable();
	}

	public async IAsyncEnumerable<TopTenDepositWithdrawInfo> GetCurrentDayDepositWithdrawTotalAmountAsync(
		IEnumerable<string> playerCodes,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var currentDay = new DateTimeOffset(
			DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8D)).Date,
			TimeSpan.FromHours(8D))
			.UtcDateTime;

		var sql = """
			SELECT
				Coalesce(DF.player_code,WF.player_code) AS PlayerCode,
				Coalesce (DF.deposit_total_amount,0) AS DepositTotalAmount,
				Coalesce( WF.withdraw_totadl_amount,0) AS WithdrawTotalAmount
			FROM (
				SELECT
					Sum(amount) AS deposit_total_amount,
					player_code
				FROM dbo.deposit_forms
				WHERE
					updated_on >= @currentDay
					AND
					status = 8000
					AND
					player_code = Any(@playerCodes)
				GROUP BY player_code
				) AS DF
			FULL OUTER JOIN
			 (
				SELECT
					sum(amount) AS withdraw_totadl_amount,
					user_id AS player_code
				FROM dbo.withdraw_forms
				WHERE updated_on >= @CurrentDay
					AND status = 9000
					AND user_type = 1
					AND
					user_id = Any(@PlayerCodes)
				GROUP BY user_id
			) AS WF on DF.player_code = WF.player_code
			""";

		var conn = tgsContext.Database.GetDbConnection();

		var result = await conn.QueryAsync<TopTenInfo>(sql, new
		{
			CurrentDay = currentDay,
			PlayerCodes = playerCodes.ToArray()
		}).ConfigureAwait(false);

		foreach (var item in result.ToList())
		{
			yield return new TopTenDepositWithdrawInfo
			{
				PlayerCode = item.PlayerCode,
				DepositTotalAmount = item.DepositTotalAmount,
				WithdrawTotalAmount = item.WithdrawTotalAmount
			};
		}
	}

	public async IAsyncEnumerable<TopTenDepositWithdrawInfo> GetCurrentDayTopTenDepositPlayersAsync(
		Guid brandId,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var currentDay = new DateTimeOffset(
			DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8D)).Date,
			TimeSpan.FromHours(8D))
			.UtcDateTime;

		var query = await tgsContext.Set<TopTenInfo>()
			.FromSqlInterpolated($"""
			SELECT
				DF.player_code,
				DF.deposit_total_amount,
				Coalesce( WF.withdraw_total_amount,0) as withdraw_total_amount
			FROM (
				SELECT
					Sum(amount) as deposit_total_amount,
					player_code
				FROM dbo.deposit_forms
				WHERE brand_id = {brandId}
					AND
					updated_on >= {currentDay}
					AND
					status = 8000
				GROUP BY player_code
				ORDER BY deposit_total_amount DESC
				LIMIT 10
				) AS DF
			LEFT JOIN
			 (
				SELECT
					Sum(amount) AS withdraw_total_amount,
					user_id AS player_code
				FROM dbo.withdraw_forms
				WHERE brand_id = {brandId}
					AND updated_on >= {currentDay}
					AND status = 9000
					AND user_type = 1
				GROUP BY user_id
				ORDER BY withdraw_total_amount DESC
			) AS WF on DF.player_code = WF.player_code
			ORDER BY DF.deposit_total_amount DESC
			""")
			.AsNoTracking()
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		foreach (var item in query)
		{
			yield return new TopTenDepositWithdrawInfo
			{
				PlayerCode = item.PlayerCode,
				DepositTotalAmount = item.DepositTotalAmount,
				WithdrawTotalAmount = item.WithdrawTotalAmount
			};
		}
	}

	public async IAsyncEnumerable<TopTenDepositWithdrawInfo> GetCurrentDayTopTenWithdrawPlayersAsync(
		Guid brandId,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var currentDay = new DateTimeOffset(
			DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8D)).Date,
			TimeSpan.FromHours(8D))
			.UtcDateTime;

		var query = await tgsContext.Set<TopTenInfo>()
			.FromSqlInterpolated($"""
			SELECT
				WF.player_code,
				Coalesce(DF.deposit_total_amount,0) AS deposit_total_amount,
				WF.withdraw_total_amount
			FROM (
				SELECT
					sum(amount) AS withdraw_total_amount,
					user_id AS player_code
				FROM dbo.withdraw_forms
				WHERE brand_id = {brandId}
					AND updated_on >= {currentDay}
					AND status = 9000
					AND user_type = 1
				GROUP BY user_id
				ORDER BY withdraw_total_amount DESC
				LIMIT 10
				) AS WF
			LEFT JOIN
			 (
				SELECT
					sum(amount) as deposit_total_amount,
					player_code
				FROM dbo.deposit_forms
				WHERE brand_id = {brandId}
					AND
					updated_on >= {currentDay}
					AND
					status = 8000
				GROUP BY player_code
				ORDER BY deposit_total_amount DESC
			) AS DF on WF.player_code = DF.player_code
			ORDER BY WF.withdraw_total_amount DESC
			""")
			.AsNoTracking()
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		foreach (var item in query)
		{
			yield return new TopTenDepositWithdrawInfo
			{
				PlayerCode = item.PlayerCode,
				DepositTotalAmount = item.DepositTotalAmount,
				WithdrawTotalAmount = item.WithdrawTotalAmount
			};
		}
	}

	public async ValueTask<DepositFormSnapshot?> GetCurrentDepositFormSnapshotAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var form = await tgsContext.DepositForms.FindAsync(
			[id],
			cancellationToken: cancellationToken).ConfigureAwait(false);

		return form is null
			? null
			: new DepositFormSnapshot(
			form.Id,
			form.PlayerCode,
			form.BrandId,
			form.BrandPaymentId,
			form.Amount,
			form.ActualAmount,
			form.Remark,
			(PaymentFlowState)form.Status,
			form.ApplyInfo,
			form.PaymentReturn,
			form.ThirdPartyOrderNo,
			form.OrderNo,
			form.ReviewedUserId,
			form.ReviewedUserName,
			form.ReviewedOn,
			form.PromotionId,
			form.PromotionPlayerId,
			form.CreatedOn,
			form.HandlingFee);
	}

	public async ValueTask<WithdrawFormSnapshot?> GetCurrentWithdrawFormSnapshotAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var form = await tgsContext.WithdrawForms.FindAsync(
			[id],
			cancellationToken: cancellationToken).ConfigureAwait(false);

		return form is null
			? null
			: new WithdrawFormSnapshot(
			form.Id,
			form.BrandId,
			form.UserId,
			form.UserName,
			(UserType)form.UserType,
			form.Amount,
			form.ActualAmount,
			form.HandlingFee,
			form.BrandPaymentId,
			(PaymentFlowState)form.Status,
			form.Remark,
			form.PlayerRemark,
			form.Note,
			form.BankInfoId,
			form.BankInfo,
			form.ApplyInfo,
			form.PaymentReturn,
			form.OrderNo,
			form.ThirdPartyOrderNo,
			form.CreatedOn,
			form.ReviewedUserId,
			form.ReviewedUserName,
			form.ReviewedOn,
			form.PaidUserId,
			form.PaidUserName,
			form.PaidOn);
	}

	public async ValueTask<DepositFormInfo?> GetDepositFormByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var query = from df in tgsContext.DepositForms
						.Include(f => f.PlayerCodeNavigation)
						.Include(f => f.BrandPayment)
						.ThenInclude(bp => bp.PaymentMethodChannel)
						.AsQueryable()
					where df.Id == id
					select new DepositFormInfo(
						df.Id,
						df.PlayerCode,
						df.PlayerCodeNavigation.Username,
						df.BrandId,
						df.BrandPaymentId,
						df.BrandPayment.ChannelName,
						df.BrandPayment.PaymentMethodChannel.MethodId,
						df.Amount,
						df.ActualAmount,
						df.Remark,
						(PaymentFlowState)df.Status,
						df.ApplyInfo,
						df.PaymentReturn,
						df.ThirdPartyOrderNo,
						df.OrderNo,
						df.ReviewedUserName,
						df.ReviewedOn,
						df.PromotionId,
						df.PromotionPlayerId,
						df.CreatedOn,
						df.UpdatedOn,
						df.HandlingFee);

		return await query.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask<PaymentInfo> GetPaymentAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var payment = await tgsContext.PaymentItems
			.FindAsync([id], cancellationToken)
			.ConfigureAwait(false);

		return new PaymentInfo(
			payment!.Id,
			payment.Code);
	}

	public IAsyncEnumerable<PaymentMethodChannelInfo> GetPaymentMethodChannelsAsync(CancellationToken cancellationToken = default)
		=> tgsContext.PaymentMethodChannels.ToAsyncEnumerable()
			.Select(pmc => new PaymentMethodChannelInfo(
				pmc.Id,
				pmc.PaymentId,
				pmc.MethodId,
				pmc.ChannelId));

	public IAsyncEnumerable<string> GetPlayerCodesByBankInfoTagAsync(
		Guid brandId,
		string tagType,
		string tag,
		CancellationToken cancellationToken = default)
		=> tgsContext.PlayerBankInfoTags
			.Where(pbit => pbit.BrandId == brandId
				&& pbit.TagType == tagType
				&& pbit.Tag == tag)
			.Select(pbit => pbit.PlayerCode)
			.ToAsyncEnumerable();

	public async ValueTask<PaymentOverview?> GetPlayerPaymentOverviewAsync(string playerCode, CancellationToken cancellationToken = default)
	{
		var overview = await tgsContext.PlayerPaymentOverviews.FindAsync(
			[playerCode],
			cancellationToken: cancellationToken).ConfigureAwait(false);

		return overview is null
			? null
			: new PaymentOverview(
				overview.FirstDepositAmount,
				overview.FirstDepositHandlingFee,
				overview.FirstDepositOn,
				overview.FirstWithdrawAmount,
				overview.FirstWithdrawHandlingFee,
				overview.FirstWithdrawOn,
				overview.RecordedBy,
				overview.TotalDepositAmount,
				overview.TotalDepositCount,
				overview.TotalWithdrawAmount,
				overview.TotalWithdrawCount);
	}

	public async ValueTask<DepositFormsStatusCount> GetReportDateDepositFormSummaryAsync(Guid brandId, DateOnly reportDate, CancellationToken cancellationToken = default)
	{
		var result = await tgsContext.BrandDailyPaymentStatistics
			.Where(p => p.BrandId == brandId && p.ReportDate == reportDate)
			.AsNoTracking()
			.FirstOrDefaultAsync(cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		return new DepositFormsStatusCount
		{
			Approved = result?.DailyDepositApprovedCount ?? 0,
			Reject = result?.DailyDepositRejectCount ?? 0,
			Total = result?.DailyDepositTotalCount ?? 0
		};
	}

	public async ValueTask<WithdrawFormInfo?> GetWithdrawFormByIdAsync(Guid formId, CancellationToken cancellationToken = default)
	{
		var query = from wf in tgsContext.WithdrawForms
						.Include(f => f.BrandPayment!)
						.ThenInclude(bp => bp.PaymentMethodChannel)
						.AsQueryable()
					where wf.Id == formId
					select new WithdrawFormInfo(
						wf.Id,
						wf.UserId,
						(UserType)wf.UserType,
						wf.UserName,
						wf.BrandId,
						wf.Amount,
						wf.ActualAmount,
						(PaymentFlowState)wf.Status,
						wf.Remark,
						wf.PlayerRemark,
						wf.ReviewedUserName,
						wf.ReviewedOn,
						wf.OrderNo,
						wf.BankInfoId,
						wf.CreatedOn,
						wf.UpdatedOn,
						wf.PaidUserId,
						wf.PaidUserName,
						wf.PaidOn,
						wf.HandlingFee,
						wf.BrandPaymentId,
						wf.BrandPayment!.ChannelName,
						wf.BrandPayment.PaymentMethodChannel.MethodId,
						wf.BankInfo,
						wf.ApplyInfo,
						wf.PaymentReturn,
						wf.ThirdPartyOrderNo,
						wf.Note);

		return await query.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
	}

	public IAsyncEnumerable<BrandPaymentInfo> QueryBrandPaymentsAsync(
		Guid? brandId,
		TransferType? transferType,
		bool? enabled,
		string? keyword,
		CancellationToken cancellationToken = default)
	{
		var query = from bp in tgsContext.BrandPayments.AsQueryable()
						.Include(bp => bp.PaymentMethodChannel)
						.ThenInclude(pmc => pmc.Payment)
						.Include(bp => bp.PaymentMethodChannel)
						.ThenInclude(pmc => pmc.Method)
						.Include(bp => bp.PaymentMethodChannel)
						.ThenInclude(pmc => pmc.Channel)
						.AsAsyncEnumerable()
					select bp;

		if (brandId != null)
			query = query.Where(bp => bp.BrandId == brandId);

		if (transferType != null)
			query = query.Where(bp => bp.TransferType == (int)transferType);

		if (enabled != null)
		{
			var paymentStatus = enabled.Value ? 0 : 1;
			query = query.Where(bp => bp.PaymentStatus == paymentStatus);
		}

		if (!string.IsNullOrWhiteSpace(keyword))
		{
			var trimKeyword = keyword.Trim();
			query = query.Where(bp => bp.ChannelName.Contains(trimKeyword)
				|| bp.PaymentMethodChannel.Payment.Code == trimKeyword
				|| bp.PaymentMethodChannel.Method.Code == trimKeyword
				|| bp.PaymentMethodChannel.Channel.Code == trimKeyword);
		}

		return query.Select(bp => new BrandPaymentInfo(
			bp.Id,
			bp.PaymentMethodChannelId,
			bp.PaymentMethodChannel.PaymentId,
			bp.PaymentMethodChannel.MethodId,
			bp.PaymentMethodChannel.ChannelId,
			bp.ChannelName,
			bp.PaymentMethodChannel.Channel.Code,
			bp.ExtendedData,
			new ValueRange<decimal>
			{
				From = bp.TransferLimitFrom,
				To = bp.TransferLimitTo
			},
			bp.AllPlayers,
			bp.IdentityAuthentication,
			bp.PhoneAuthentication,
			new ValueRange<decimal>
			{
				From = bp.DepositedAmountMin,
				To = bp.DepositedAmountUnlimited ? decimal.MaxValue : bp.DepositedAmountMax
			},
			bp.PlayerListCondition == 1 ? ConditionMatchType.MatchOne : ConditionMatchType.MatchAll,
			bp.TransferType switch
			{
				1 => TransferType.Withdraw,
				_ => TransferType.Deposit
			},
			bp.PaymentStatus == 0,
			bp.DisplayOrder,
			bp.BrandId));
	}

	public IAsyncEnumerable<DepositFormInfo> QueryDepositFormsAsync(
		DepositFormQueryConditions conditions,
		Paging<DepositFormInfo> paging,
		CancellationToken cancellationToken = default)
	{
		var dataQuery = MakeDepositFormQuery(conditions)
			.Select(form => new DepositFormInfo(
				form.Id,
				form.PlayerCode,
				form.PlayerCodeNavigation.Username,
				form.BrandId,
				form.BrandPaymentId,
				form.BrandPayment.ChannelName,
				form.BrandPayment.PaymentMethodChannel.MethodId,
				form.Amount,
				form.ActualAmount,
				form.Remark,
				(PaymentFlowState)form.Status,
				form.ApplyInfo,
				form.PaymentReturn,
				form.ThirdPartyOrderNo,
				form.OrderNo,
				form.ReviewedUserName,
				form.ReviewedOn,
				form.PromotionId,
				form.PromotionPlayerId,
				form.CreatedOn,
				form.UpdatedOn,
				form.HandlingFee))
			.ToAsyncEnumerable();

		dataQuery = dataQuery.ApplyOrderBy(paging);

		if (paging.RowCount > 0)
			dataQuery = dataQuery.ApplyPaging(paging);

		return dataQuery;
	}

	public IAsyncEnumerable<WithdrawFormInfo> QueryWithdrawFormsAsync(
		WithdrawFormQueryConditions conditions,
		Paging<WithdrawFormInfo> paging,
		CancellationToken cancellationToken = default)
	{
		var dataQuery = MakeWithdrawFormQueryWithLimit90Days(conditions)
			.Select(form => new WithdrawFormInfo(
				form.Id,
				form.UserId,
				(UserType)form.UserType,
				form.UserName,
				form.BrandId,
				form.Amount,
				form.ActualAmount,
				(PaymentFlowState)form.Status,
				form.Remark,
				form.PlayerRemark,
				form.ReviewedUserName,
				form.ReviewedOn,
				form.OrderNo,
				form.BankInfoId,
				form.CreatedOn,
				form.UpdatedOn,
				form.PaidUserId,
				form.PaidUserName,
				form.PaidOn,
				form.HandlingFee,
				form.BrandPaymentId,
				form.BrandPayment!.ChannelName,
				form.BrandPayment.PaymentMethodChannel.MethodId,
				form.BankInfo,
				form.ApplyInfo,
				form.PaymentReturn,
				form.ThirdPartyOrderNo,
				form.Note))
			.ToAsyncEnumerable();

		dataQuery = dataQuery.ApplyOrderBy(paging);

		if (paging.RowCount > 0)
			dataQuery = dataQuery.ApplyPaging(paging);

		return dataQuery;
	}

	public IAsyncEnumerable<WithdrawFormInfo> QueryWithdrawFormsAsync(
		WithdrawFormQueryConditions conditions,
		bool limitQuery = true,
		CancellationToken cancellationToken = default)
	{
		var dataQuery = limitQuery
			? MakeWithdrawFormQueryWithLimit90Days(conditions)
			: MakeWithdrawFormQuery(conditions);

		return dataQuery
			.Select(form => new WithdrawFormInfo(
				form.Id,
				form.UserId,
				(UserType)form.UserType,
				form.UserName,
				form.BrandId,
				form.Amount,
				form.ActualAmount,
				(PaymentFlowState)form.Status,
				form.Remark,
				form.PlayerRemark,
				form.ReviewedUserName,
				form.ReviewedOn,
				form.OrderNo,
				form.BankInfoId,
				form.CreatedOn,
				form.UpdatedOn,
				form.PaidUserId,
				form.PaidUserName,
				form.PaidOn,
				form.HandlingFee,
				form.BrandPaymentId,
				form.BrandPayment!.ChannelName,
				form.BrandPayment.PaymentMethodChannel.MethodId,
				form.BankInfo,
				form.ApplyInfo,
				form.PaymentReturn,
				form.ThirdPartyOrderNo,
				form.Note))
			.ToAsyncEnumerable();
	}

	public async ValueTask RemoveBankInfoTagsAsync(Guid brandId, Guid bankInfoId, CancellationToken cancellationToken = default)
	{
		var tags = await tgsContext.PlayerBankInfoTags
			.Where(bt => bt.BrandId == brandId
				&& bt.BankInfoId == bankInfoId)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		tgsContext.PlayerBankInfoTags.RemoveRange(tags);

		_ = await tgsContext
			.SaveChangesAsync(cancellationToken)
			.ConfigureAwait(false);
	}

	public async ValueTask SetBankInfoTagAsync(
		Guid brandId,
		Guid bankInfoId,
		string tagType,
		string playerCode,
		string tag,
		CancellationToken cancellationToken = default)
	{
		var insertTag = new PlayerBankInfoTag
		{
			BrandId = brandId,
			BankInfoId = bankInfoId,
			TagType = tagType,
			PlayerCode = playerCode,
			Tag = tag
		};

		_ = await tgsContext.PlayerBankInfoTags
			.AddAsync(insertTag, cancellationToken)
			.ConfigureAwait(false);

		_ = await tgsContext
			.SaveChangesAsync(cancellationToken)
			.ConfigureAwait(false);
	}

	public async ValueTask SetBrandCheckPlayerBankInfoDuplicateEnabledAsync(Guid brandId, bool brandEnabled, CancellationToken cancellationToken = default)
	{
		var flag = await tgsContext.BrandPlayerBankinfoCheckFlags.FindAsync(
			[brandId],
			cancellationToken: cancellationToken).ConfigureAwait(false)
			?? (await tgsContext.BrandPlayerBankinfoCheckFlags
				.AddAsync(new BrandPlayerBankinfoCheckFlag { BrandId = brandId }, cancellationToken)
				.ConfigureAwait(false)).Entity;

		flag.IsEnabled = brandEnabled;

		_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask SetDefaultPlayerBankInfoAsync(
			string playerCode,
		Guid bankInfoId,
		string modifyUser,
		CancellationToken cancellationToken = default)
	{
		var bankInfos = await tgsContext.PlayerBankInfos
			.Where(b => b.PlayerCode == playerCode)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		var bankInfo = bankInfos.FirstOrDefault(b => b.Id == bankInfoId);
		if (bankInfo == null || bankInfo.Default)
			return;

		var updateTime = DateTime.UtcNow;

		foreach (var info in bankInfos)
		{
			if (info.Id != bankInfoId && !info.Default)
				continue;

			info.Default = info.Id == bankInfoId;
			info.UpdatedOn = updateTime;
			info.UpdatedByUsername = modifyUser;
		}

		_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask UpdateBrandAutoWithdrawSnapshotAsync(
		AutoWithdrawInfo snapshot,
		Guid brandId,
		Guid snapshotId,
		DateTime updatedOn,
		CancellationToken cancellationToken = default)
	{
		var dbSnapshot = await tgsContext.BrandPaymentAutoWithdraws
			.FindAsync([brandId], cancellationToken: cancellationToken)
			.ConfigureAwait(false)
			?? (await tgsContext.BrandPaymentAutoWithdraws.AddAsync(new BrandPaymentAutoWithdraw
			{
				BrandId = brandId,
				AmountUpperLimit = snapshot.AmountUpperLimit,
				BrandPaymentCandidates = JsonSerializer.Serialize(new { brandPaymentIds = snapshot.BrandPaymentIds }),
				EnableAutoWithdraw = snapshot.EnableAutoWithdraw,
				SnapshotEventId = snapshotId,
			}, cancellationToken).ConfigureAwait(false)).Entity;

		if (dbSnapshot.SnapshotEventId.CompareTo(snapshotId) == -1)
		{
			dbSnapshot.AmountUpperLimit = snapshot.AmountUpperLimit;
			dbSnapshot.EnableAutoWithdraw = snapshot.EnableAutoWithdraw;
			dbSnapshot.BrandPaymentCandidates = JsonSerializer.Serialize(new { brandPaymentIds = snapshot.BrandPaymentIds });
			dbSnapshot.SnapshotEventId = snapshotId;
		}

		_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask UpdateBrandPaymentExtendedDataAsync(Guid brandPaymentId, JsonDocument extendedData, CancellationToken cancellationToken = default)
	{
		var brandPayment = await tgsContext.BrandPayments
			.FindAsync([brandPaymentId], cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (brandPayment is null)
			return;

		brandPayment.ExtendedData = JsonSerializer.Serialize(extendedData);

		_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask UpdateDepositFormSnapshotAsync(
		DepositFormSnapshot snapshot,
		Guid snapshotId,
		DateTime updatedOn,
		CancellationToken cancellationToken = default)
	{
		var dbSnapshot = await tgsContext.DepositForms
			.FindAsync([snapshot.Id], cancellationToken: cancellationToken)
			.ConfigureAwait(false)
			?? (await tgsContext.DepositForms.AddAsync(new DepositForm
			{
				Id = snapshot.Id
			}, cancellationToken).ConfigureAwait(false)).Entity;

		if (dbSnapshot.SnapshotEventId is null || dbSnapshot.SnapshotEventId.Value.CompareTo(snapshotId) == -1)
		{
			dbSnapshot.ActualAmount = snapshot.ActualAmount;
			dbSnapshot.Amount = snapshot.Amount;
			dbSnapshot.ApplyInfo = snapshot.ApplyInfo;
			dbSnapshot.BrandId = snapshot.BrandId;
			dbSnapshot.BrandPaymentId = snapshot.BrandPaymentId;
			dbSnapshot.CreatedOn = snapshot.CreatedOn;
			dbSnapshot.HandlingFee = snapshot.HandlingFee;
			dbSnapshot.OrderNo = snapshot.OrderNo;
			dbSnapshot.PaymentReturn = snapshot.PaymentReturn;
			dbSnapshot.PlayerCode = snapshot.PlayerCode;
			dbSnapshot.PromotionId = snapshot.PromotionId;
			dbSnapshot.PromotionPlayerId = snapshot.PromotionPlayerId;
			dbSnapshot.Remark = snapshot.Remark;
			dbSnapshot.ReviewedOn = snapshot.ReviewedOn;
			dbSnapshot.ReviewedUserId = snapshot.ReviewedUserId;
			dbSnapshot.ReviewedUserName = snapshot.ReviewedUserName;
			dbSnapshot.Status = (int)snapshot.Status;
			dbSnapshot.ThirdPartyOrderNo = snapshot.ThirdPartyOrderNo;
			dbSnapshot.SnapshotEventId = snapshotId;
			dbSnapshot.UpdatedOn = updatedOn;
		}

		_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask UpdatePlayerBankInfoHashAsync(Guid bankInfoId, byte[]? hash, CancellationToken cancellationToken = default)
	{
		var bankInfo = await tgsContext.PlayerBankInfos
			.FindAsync([bankInfoId], cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (bankInfo is null)
			return;

		bankInfo.BankInfoHash = hash;

		_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask UpdateWithdrawFormSnapshotAsync(
		WithdrawFormSnapshot snapshot,
		Guid snapshotId,
		DateTime updatedOn,
		CancellationToken cancellationToken = default)
	{
		var dbSnapshot = await tgsContext.WithdrawForms
			.FindAsync([snapshot.Id], cancellationToken: cancellationToken)
			.ConfigureAwait(false)
			?? (await tgsContext.WithdrawForms.AddAsync(new WithdrawForm
			{
				Id = snapshot.Id
			}, cancellationToken).ConfigureAwait(false)).Entity;

		if (dbSnapshot.SnapshotEventId is null || dbSnapshot.SnapshotEventId.Value.CompareTo(snapshotId) == -1)
		{
			dbSnapshot.BrandId = snapshot.BrandId;
			dbSnapshot.UserId = snapshot.UserId;
			dbSnapshot.UserName = snapshot.UserName;
			dbSnapshot.UserType = (int)snapshot.UserType;
			dbSnapshot.Amount = snapshot.Amount;
			dbSnapshot.ActualAmount = snapshot.ActualAmount;
			dbSnapshot.HandlingFee = snapshot.HandlingFee;
			dbSnapshot.ApplyInfo = snapshot.ApplyInfo;
			dbSnapshot.BrandPaymentId = snapshot.BrandPaymentId;
			dbSnapshot.Status = (int)snapshot.Status;
			dbSnapshot.Remark = snapshot.Remark;
			dbSnapshot.PlayerRemark = snapshot.PlayerRemark;
			dbSnapshot.Note = snapshot.Note;
			dbSnapshot.BankInfoId = snapshot.BankInfoId;
			dbSnapshot.BankInfo = snapshot.BankInfo;
			dbSnapshot.ApplyInfo = snapshot.ApplyInfo;
			dbSnapshot.PaymentReturn = snapshot.PaymentReturn;
			dbSnapshot.OrderNo = snapshot.OrderNo;
			dbSnapshot.ThirdPartyOrderNo = snapshot.ThirdPartyOrderNo;
			dbSnapshot.CreatedOn = snapshot.CreatedOn;
			dbSnapshot.ReviewedUserId = snapshot.ReviewedUserId;
			dbSnapshot.ReviewedUserName = snapshot.ReviewedUserName;
			dbSnapshot.ReviewedOn = snapshot.ReviewedOn;
			dbSnapshot.PaidUserId = snapshot.PaidUserId;
			dbSnapshot.PaidUserName = snapshot.PaidUserName;
			dbSnapshot.PaidOn = snapshot.PaidOn;

			dbSnapshot.SnapshotEventId = snapshotId;
			dbSnapshot.UpdatedOn = updatedOn;
		}

		_ = await tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask UpsertBrandPaymentSettingsAsync(
		Guid brandId,
		BrandPaymentSettings settings,
		CancellationToken cancellationToken = default)
	{
		var row = (await tgsContext.BrandPaymentSettings.FindAsync(
			[brandId],
			cancellationToken: cancellationToken).ConfigureAwait(false))
			?? tgsContext.BrandPaymentSettings
				.Add(new BrandPaymentSetting
				{
					BrandId = brandId
				})
				.Entity;

		row.PlayerBankInfoCountLimit = settings.PlayerBankInfoCountLimit;

		_ = await tgsContext.SaveChangesAsync(
			cancellationToken).ConfigureAwait(false);
	}

	private IQueryable<DepositForm> MakeDepositFormQuery(DepositFormQueryConditions conditions)
	{
		var currentDate = DateTimeOffset.UtcNow;

		var dataQuery = tgsContext.DepositForms.AsQueryable()
			.Include(f => f.PlayerCodeNavigation)
			.Include(f => f.BrandPayment)
			.ThenInclude(bp => bp.PaymentMethodChannel)
			.Where(form => form.BrandId == conditions.BrandId)
			.Where(form => form.CreatedOn >= currentDate.AddDays(-90)
				&& form.CreatedOn < currentDate);

		if (!string.IsNullOrWhiteSpace(conditions.UserId))
			dataQuery = dataQuery.Where(form => form.PlayerCode == conditions.UserId);

		if (!string.IsNullOrWhiteSpace(conditions.UserName))
			dataQuery = dataQuery.Where(form => form.PlayerCodeNavigation.Username == conditions.UserName);

		if (!string.IsNullOrWhiteSpace(conditions.ChannelName))
			dataQuery = dataQuery.Where(form => form.BrandPayment.ChannelName == conditions.ChannelName);

		if (conditions.PaymentId != null)
			dataQuery = dataQuery.Where(form => form.BrandPayment.PaymentMethodChannel.PaymentId == conditions.PaymentId);

		if (conditions.PaymentMethodId != null)
			dataQuery = dataQuery.Where(form => form.BrandPayment.PaymentMethodChannel.MethodId == conditions.PaymentMethodId);

		if (!string.IsNullOrWhiteSpace(conditions.ThirdPartyOrderNo))
			dataQuery = dataQuery.Where(form => form.ThirdPartyOrderNo == conditions.ThirdPartyOrderNo);

		if (!string.IsNullOrWhiteSpace(conditions.OrderNo))
			dataQuery = dataQuery.Where(form => form.OrderNo == conditions.OrderNo);

		if (conditions.FormId != null)
			dataQuery = dataQuery.Where(form => form.Id == conditions.FormId);

		if (conditions.Status != null)
		{
			var statusList = conditions.Status.Cast<int>().ToArray();

			if (statusList.Length > 0)
				dataQuery = dataQuery.Where(form => statusList.Contains(form.Status));
		}

		if (conditions.CreateOnRange != null)
			dataQuery = dataQuery.Where(form => form.CreatedOn >= conditions.CreateOnRange.Value.Begin
				&& form.CreatedOn < conditions.CreateOnRange.Value.End);

		if (conditions.UpdateOnRange != null)
			dataQuery = dataQuery.Where(form => form.UpdatedOn >= conditions.UpdateOnRange.Value.Begin
				&& form.UpdatedOn < conditions.UpdateOnRange.Value.End);

		return dataQuery;
	}

	private IQueryable<WithdrawForm> MakeWithdrawFormQuery(WithdrawFormQueryConditions conditions)
	{
		var dataQuery = tgsContext.WithdrawForms.AsQueryable()
			.Include(f => f.BrandPayment)
			.ThenInclude(bp => bp!.PaymentMethodChannel)
			.Where(form => form.BrandId == conditions.BrandId);

		if (!string.IsNullOrWhiteSpace(conditions.UserId))
			dataQuery = dataQuery.Where(form => form.UserId == conditions.UserId);

		if (conditions.UserType != null)
		{
			var userType = (int)conditions.UserType;
			dataQuery = dataQuery.Where(form => form.UserType == userType);
		}

		if (!string.IsNullOrWhiteSpace(conditions.UserName))
			dataQuery = dataQuery.Where(form => form.UserName == conditions.UserName);

		if (!string.IsNullOrWhiteSpace(conditions.ChannelName))
			dataQuery = dataQuery.Where(form => form.BrandPayment!.ChannelName == conditions.ChannelName);

		if (conditions.PaymentId != null)
			dataQuery = dataQuery.Where(
				form => form.BrandPayment!.PaymentMethodChannel.PaymentId == conditions.PaymentId);

		if (conditions.PaymentMethodId != null)
			dataQuery = dataQuery.Where(
				 form => form.BrandPayment!.PaymentMethodChannel.MethodId == conditions.PaymentMethodId);

		if (!string.IsNullOrWhiteSpace(conditions.ThirdPartyOrderNo))
			dataQuery = dataQuery.Where(
				form => form.ThirdPartyOrderNo == conditions.ThirdPartyOrderNo);

		if (!string.IsNullOrWhiteSpace(conditions.OrderNo))
			dataQuery = dataQuery.Where(
				form => form.OrderNo == conditions.OrderNo);
		if (conditions.FormId != null)
			dataQuery = dataQuery.Where(form => form.Id == conditions.FormId);

		var statusList = conditions.Status.Cast<int>().ToArray();

		if (statusList.Length > 0)
			dataQuery = dataQuery.Where(form => statusList.Contains(form.Status));

		var brandIdList = conditions.BrandPaymentIds.ToArray();

		if (brandIdList.Length > 0)
			dataQuery = dataQuery.Where(form => brandIdList.Contains(form.BrandPaymentId!.Value));

		if (conditions.CreateOnRange != null)
			dataQuery = dataQuery.Where(form => form.CreatedOn >= conditions.CreateOnRange.Value.Begin
				&& form.CreatedOn < conditions.CreateOnRange.Value.End);

		if (conditions.UpdateOnRange != null)
			dataQuery = dataQuery.Where(form => form.UpdatedOn >= conditions.UpdateOnRange.Value.Begin
				&& form.UpdatedOn < conditions.UpdateOnRange.Value.End);

		if (conditions.ReviewOnRange != null)
			dataQuery = dataQuery.Where(form => form.ReviewedOn >= conditions.ReviewOnRange.Value.Begin
				&& form.ReviewedOn < conditions.ReviewOnRange.Value.End);

		if (conditions.PaidOnRange != null)
			dataQuery = dataQuery.Where(form => form.PaidOn >= conditions.PaidOnRange.Value.Begin
				&& form.PaidOn < conditions.PaidOnRange.Value.End);

		return dataQuery;
	}

	private IQueryable<WithdrawForm> MakeWithdrawFormQueryWithLimit90Days(WithdrawFormQueryConditions conditions)
	{
		var currentDate = DateTimeOffset.UtcNow;

		return MakeWithdrawFormQuery(conditions)
			.Where(form => form.CreatedOn >= currentDate.AddDays(-90)
				&& form.CreatedOn < currentDate);
	}
}