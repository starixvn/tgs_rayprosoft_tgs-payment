using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.Payment;
using Tgs.Payment.Repositories.Entities;
using TGS.Paging;

namespace TGS.Payment.Repositories;

internal class PaymentRepository : IPaymentRepository
{
	private readonly TgsContext _tgsContext;

	public PaymentRepository(TgsContext tgsContext)
	{
		_tgsContext = tgsContext ?? throw new ArgumentNullException(nameof(tgsContext));
	}

	public async ValueTask<decimal> CalculateDepositTotalAmountAsync(DepositFormQueryConditions conditions, CancellationToken cancellationToken = default)
	{
		var dataQuery = MakeDepositFormQuery(conditions);

		return await dataQuery
			.SumAsync(x => x.ActualAmount ?? x.Amount, cancellationToken)
			.ConfigureAwait(false);
	}

	public async ValueTask<decimal> CalculateWithdrawTotalAmountAsync(WithdrawFormQueryConditions conditions, CancellationToken cancellationToken = default)
	{
		var dataQuery = MakeWithdrawFormQuery(conditions);

		return await dataQuery
			.SumAsync(x => x.ActualAmount ?? x.Amount, cancellationToken)
			.ConfigureAwait(false);
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
		var dataQuery = MakeWithdrawFormQuery(conditions);

		return await dataQuery
			.CountAsync(cancellationToken)
			.ConfigureAwait(false);
	}

	public async ValueTask CreateAgentBankInfoAsync(AgentBankInfo agentBankInfo, string modifyUser, CancellationToken cancellationToken = default)
	{
		var createTime = DateTimeOffset.UtcNow;

		_ = await _tgsContext.AgentBankInfoItems
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

		_ = await _tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask CreateBankInfoAsync(Tgs.Payment.PlayerBankInfo bankInfo, string modifyUser, CancellationToken cancellationToken = default)
	{
		var createTime = DateTime.UtcNow;

		_ = await _tgsContext.PlayerBankInfos
			.AddAsync(new Tgs.Payment.Repositories.Entities.PlayerBankInfo
			{
				Id = bankInfo.Id,
				PlayerCode = bankInfo.PlayerCode,
				BrandId = bankInfo.BrandId,
				PaymentChannelId = bankInfo.PaymentChannelId,
				BankType = bankInfo.BankType,
				Detail = bankInfo.Detail,
				CreatedByUsername = modifyUser,
				CreatedOn = createTime,
				UpdatedByUsername = modifyUser,
				UpdatedOn = createTime
			}, cancellationToken)
			.ConfigureAwait(false);

		_ = await _tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask CreateDepositFormAsync(Guid createFormId, DepositFormCreateInfo form, CancellationToken cancellationToken = default)
	{
		var currentDate = DateTime.UtcNow;

		_ = await _tgsContext.DepositForms
			.AddAsync(new DepositForm
			{
				Id = createFormId,
				PlayerCode = form.PlayerCode,
				BrandId = form.BrandId,
				BrandPaymentId = form.BrandPaymentId,
				Amount = form.Amount,
				ApplyInfo = form.ApplyInfo,
				PaymentReturn = form.PaymentReturn,
				PromotionId = form.PromotionId,
				CreatedOn = currentDate,
				UpdatedOn = currentDate,
				OrderNo = form.OrderNo
			}, cancellationToken)
			.ConfigureAwait(false);

		_ = await _tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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

		_ = await _tgsContext.PaymentItems
			.AddAsync(createPayment, cancellationToken)
			.ConfigureAwait(false);

		_ = await _tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask CreatePaymentChannelAsync(string channelCode, CancellationToken cancellationToken = default)
	{
		var createChannel = new PaymentChannel
		{
			Id = Guid.NewGuid(),
			Code = channelCode
		};

		_ = await _tgsContext.PaymentChannels
			.AddAsync(createChannel, cancellationToken)
			.ConfigureAwait(false);

		_ = await _tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask CreatePaymentMethodAsync(string methodCode, string bankType, CancellationToken cancellationToken = default)
	{
		var createMethod = new PaymentMethod
		{
			Id = Guid.NewGuid(),
			Code = methodCode,
			BankType = bankType
		};

		_ = await _tgsContext.PaymentMethods
			.AddAsync(createMethod, cancellationToken)
			.ConfigureAwait(false);

		_ = await _tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask CreateWithdrawFormAsync(Guid createFormId, WithdrawFormCreateInfo form, CancellationToken cancellationToken = default)
	{
		var currentDate = DateTime.UtcNow;

		_ = await _tgsContext.WithdrawForms
			.AddAsync(new WithdrawForm
			{
				Id = createFormId,
				BrandId = form.BrandId,
				UserId = form.UserId,
				UserName = form.UserName,
				UserType = (int)form.UserType,
				BrandPaymentId = form.BrandPaymentId,
				Amount = form.Amount,
				BankInfoId = form.BankInfoId,
				BankInfo = form.BankInfo!,
				ApplyInfo = form.ApplyInfo,
				PaymentReturn = form.PaymentReturn,
				Status = (int)form.Status,
				CreatedOn = currentDate,
				UpdatedOn = currentDate,
				OrderNo = form.OrderNo
			}, cancellationToken)
			.ConfigureAwait(false);

		_ = await _tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask DeleteAgentBankInfoAsync(Guid agentId, Guid bankInfoId, CancellationToken cancellationToken = default)
	{
		var bankInfo = await _tgsContext.AgentBankInfoItems
			.FindAsync(new object?[] { bankInfoId }, cancellationToken)
			.ConfigureAwait(false);

		if (bankInfo != null && bankInfo.AgentId == agentId)
		{
			_ = _tgsContext.AgentBankInfoItems.Remove(bankInfo);

			_ = await _tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	public async ValueTask DeleteBankInfoAsync(string playerCode, Guid bankInfoId, CancellationToken cancellationToken = default)
	{
		var bankInfo = await _tgsContext.PlayerBankInfos
			.FindAsync(new object?[] { bankInfoId }, cancellationToken)
			.ConfigureAwait(false);

		if (bankInfo != null && bankInfo.PlayerCode == playerCode)
		{
			_ = _tgsContext.PlayerBankInfos.Remove(bankInfo);

			_ = await _tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	public async ValueTask<AgentBankInfo> GetAgentBankInfoByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var bankInfo = await _tgsContext.AgentBankInfoItems
			.FindAsync(new object?[] { id }, cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		return new AgentBankInfo(
			bankInfo!.Id,
			bankInfo.AgentId,
			bankInfo.BrandId,
			bankInfo.PaymentChannelId,
			bankInfo.BankType,
			bankInfo.Detail!);
	}

	public IAsyncEnumerable<Tgs.Payment.AgentBankInfo> GetAgentBankInfosByAgentIdAsync(Guid agentId, CancellationToken cancellationToken = default)
		=> _tgsContext.AgentBankInfoItems.AsQueryable()
			.Where(info => info.AgentId == agentId)
			.Select(info => new AgentBankInfo(
				info.Id,
				info.AgentId,
				info.BrandId,
				info.PaymentChannelId,
				info.BankType,
				info.Detail!))
			.ToAsyncEnumerable();

	public IAsyncEnumerable<PaymentChannelInfo> GetAllPaymentChannelsAsync(CancellationToken cancellationToken = default)
		=> _tgsContext.PaymentChannels.AsQueryable()
			.Select(row => new PaymentChannelInfo(row.Id, row.Code))
			.ToAsyncEnumerable();

	public IAsyncEnumerable<PaymentMethodChannelInfo> GetAllPaymentMethodChannelsAsync(CancellationToken cancellationToken = default)
		=> _tgsContext.PaymentMethodChannels.AsQueryable()
			.Select(row => new PaymentMethodChannelInfo(
				row.Id,
				row.PaymentId,
				row.MethodId,
				row.ChannelId))
			.ToAsyncEnumerable();

	public IAsyncEnumerable<PaymentMethodInfo> GetAllPaymentMethodsAsync(CancellationToken cancellationToken = default)
		=> _tgsContext.PaymentMethods.AsQueryable()
			.Select(row => new PaymentMethodInfo(row.Id, row.Code, row.BankType))
			.ToAsyncEnumerable();

	public IAsyncEnumerable<PaymentInfo> GetAllPaymentsAsync(CancellationToken cancellationToken = default)
		=> _tgsContext.PaymentItems.AsQueryable()
			.Select(row => new PaymentInfo(row.Id, row.Code))
			.ToAsyncEnumerable();

	public async ValueTask<Tgs.Payment.PlayerBankInfo> GetBankInfoByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var bankInfo = await _tgsContext.PlayerBankInfos
			.FindAsync(new object?[] { id }, cancellationToken)
			.ConfigureAwait(false);

		return new Tgs.Payment.PlayerBankInfo(
			bankInfo!.Id,
			bankInfo.PlayerCode,
			bankInfo.BrandId,
			bankInfo.PaymentChannelId,
			bankInfo.BankType,
			bankInfo.Detail!);
	}

	public IAsyncEnumerable<Tgs.Payment.PlayerBankInfo> GetBankInfosByPlayerCodeAsync(string playerCode, CancellationToken cancellationToken = default)
		=> _tgsContext.PlayerBankInfos.AsQueryable()
			.Where(info => info.PlayerCode == playerCode)
			.Select(info => new Tgs.Payment.PlayerBankInfo(
				info.Id,
				info.PlayerCode,
				info.BrandId,
				info.PaymentChannelId,
				info.BankType,
				info.Detail!))
			.ToAsyncEnumerable();

	public async ValueTask<BrandInfo> GetBrandInfoByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var brandInfo = await _tgsContext.Brands
			.FindAsync(new object?[] { id }, cancellationToken)
			.ConfigureAwait(false);

		return new BrandInfo(brandInfo!.Code);
	}

	public IAsyncEnumerable<BrandPaymentAcl> GetBrandPaymentAclsAsync(IEnumerable<Guid> brandPaymentIds, string playerCode, CancellationToken cancellationToken = default)
	{
		var ids = brandPaymentIds.ToArray();

		var query = from acl in _tgsContext.BrandPaymentPlayers.AsQueryable()
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
		var row = await _tgsContext.BrandPayments.AsQueryable()
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
			row.DisplayOrder);
	}

	public IAsyncEnumerable<BrandPaymentInfo> GetBrandPaymentsAsync(Guid brandId, CancellationToken cancellationToken = default)
	{
		var query = from bp in _tgsContext.BrandPayments.AsQueryable()
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
			bp.DisplayOrder));
	}

	public IAsyncEnumerable<BrandPaymentVip> GetBrandPaymentVipListAsync(IEnumerable<Guid> brandPaymentIds, CancellationToken cancellationToken = default)
	{
		var ids = brandPaymentIds.ToArray();

		return _tgsContext.BrandPaymentVipLevels.AsQueryable()
			.Where(v => ids.Contains(v.BrandPaymentId))
			.Select(v => new BrandPaymentVip(v.BrandPaymentId, v.VipLevelId))
			.ToAsyncEnumerable();
	}

	public async ValueTask<DepositFormInfo?> GetDepositFormByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var query = from df in _tgsContext.DepositForms
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
						df.PromotionId,
						df.PromotionPlayerId,
						df.CreatedOn,
						df.UpdatedOn);

		return await query.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask<PaymentInfo> GetPaymentAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var payment = await _tgsContext.PaymentItems
			.FindAsync(new object?[] { id }, cancellationToken)
			.ConfigureAwait(false);

		return new PaymentInfo(
			payment!.Id,
			payment.Code);
	}

	public IAsyncEnumerable<PaymentMethodChannelInfo> GetPaymentMethodChannelsAsync(CancellationToken cancellationToken = default)
	{
		return _tgsContext.PaymentMethodChannels.ToAsyncEnumerable()
			.Select(pmc => new PaymentMethodChannelInfo(
				pmc.Id,
				pmc.PaymentId,
				pmc.MethodId,
				pmc.ChannelId));
	}

	public async ValueTask<WithdrawFormInfo?> GetWithdrawFormByIdAsync(Guid formId, CancellationToken cancellationToken = default)
	{
		var query = from df in _tgsContext.WithdrawForms
						.Include(f => f.BrandPayment!)
						.ThenInclude(bp => bp.PaymentMethodChannel)
						.AsQueryable()
					where df.Id == formId
					select new WithdrawFormInfo(
						df.Id,
						df.UserId,
						(UserType)df.UserType,
						df.UserName,
						df.BrandId,
						df.Amount,
						df.ActualAmount,
						(PaymentFlowState)df.Status,
						df.Remark,
						df.ReviewedUserName,
						df.ReviewedOn,
						df.OrderNo,
						df.BankInfoId,
						df.CreatedOn,
						df.UpdatedOn,
						df.PaidUserId,
						df.PaidUserName,
						df.PaidOn,
						df.BrandPaymentId,
						df.BrandPayment!.ChannelName,
						df.BrandPayment.PaymentMethodChannel.MethodId,
						df.BankInfo,
						df.ApplyInfo,
						df.PaymentReturn,
						df.ThirdPartyOrderNo);

		return await query.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
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
				form.PromotionId,
				form.PromotionPlayerId,
				form.CreatedOn,
				form.UpdatedOn))
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
		var dataQuery = MakeWithdrawFormQuery(conditions)
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
				form.ReviewedUserName,
				form.ReviewedOn,
				form.OrderNo,
				form.BankInfoId,
				form.CreatedOn,
				form.UpdatedOn,
				form.PaidUserId,
				form.PaidUserName,
				form.PaidOn,
				form.BrandPaymentId,
				form.BrandPayment!.ChannelName,
				form.BrandPayment.PaymentMethodChannel.MethodId,
				form.BankInfo,
				form.ApplyInfo,
				form.PaymentReturn,
				form.ThirdPartyOrderNo))
			.ToAsyncEnumerable();

		dataQuery = dataQuery.ApplyOrderBy(paging);

		if (paging.RowCount > 0)
			dataQuery = dataQuery.ApplyPaging(paging);

		return dataQuery;
	}

	public IAsyncEnumerable<WithdrawFormInfo> QueryWithdrawFormsAsync(WithdrawFormQueryConditions conditions, CancellationToken cancellationToken = default)
	{
		var dataQuery = MakeWithdrawFormQuery(conditions);

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
				form.ReviewedUserName,
				form.ReviewedOn,
				form.OrderNo,
				form.BankInfoId,
				form.CreatedOn,
				form.UpdatedOn,
				form.PaidUserId,
				form.PaidUserName,
				form.PaidOn,
				form.BrandPaymentId,
				form.BrandPayment!.ChannelName,
				form.BrandPayment.PaymentMethodChannel.MethodId,
				form.BankInfo,
				form.ApplyInfo,
				form.PaymentReturn,
				form.ThirdPartyOrderNo))
			.ToAsyncEnumerable();
	}

	public async ValueTask SignOffDepositFormAsync(
		Guid formId,
		ReviewState reviewStatus,
		string? reviewedUserId,
		string? reviewedUserName,
		string? remark = null,
		CancellationToken cancellationToken = default)
	{
		var formData = await _tgsContext.DepositForms
			.FindAsync(new object?[] { formId }, cancellationToken)
			.ConfigureAwait(false);

		if (formData == null)
			return;

		formData.Status = reviewStatus.ToNumber();
		formData.Remark = remark;
		formData.UpdatedOn = DateTime.UtcNow;
		formData.ReviewedUserId = reviewedUserId ?? formData.ReviewedUserId;
		formData.ReviewedUserName = reviewedUserName ?? formData.ReviewedUserName;
		if (reviewedUserId != null)
		{
			formData.ReviewedOn = DateTime.UtcNow;
		}

		_ = await _tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask SignOffWithdrawFormAsync(
		Guid formId,
		Guid? brandPaymentId,
		string? applyInfo,
		PaymentFlowState afterState,
		string? reviewedUserId,
		string? reviewedUserName,
		string? remark,
		CancellationToken cancellationToken = default)
	{
		var formData = await _tgsContext.WithdrawForms
			.FindAsync(new object?[] { formId }, cancellationToken)
			.ConfigureAwait(false);

		if (formData == null)
			return;

		formData.BrandPaymentId = brandPaymentId;
		formData.ApplyInfo = applyInfo;
		formData.Status = (int)afterState;
		formData.Remark = remark;
		formData.UpdatedOn = DateTime.UtcNow;
		formData.ReviewedUserId = reviewedUserId ?? formData.ReviewedUserId;
		formData.ReviewedUserName = reviewedUserName ?? formData.ReviewedUserName;
		if (reviewedUserId != null)
			formData.ReviewedOn = DateTime.UtcNow;

		_ = await _tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask SubmitDepositFormStatusAsync(
		Guid formId,
		PaymentFlowState? status,
		string? paymentReturn,
		decimal? actualAmount,
		string? thirdPartyOrderNo,
		int? promotionPlayerId,
		CancellationToken cancellationToken = default)
	{
		var formData = await _tgsContext.DepositForms
			.FindAsync(new object?[] { formId }, cancellationToken)
			.ConfigureAwait(false);

		if (formData == null)
			return;

		formData.Status = status.HasValue ? (int)status.Value : formData.Status;
		formData.PaymentReturn = paymentReturn ?? formData.PaymentReturn;
		formData.ActualAmount = actualAmount ?? formData.ActualAmount;
		formData.ThirdPartyOrderNo = thirdPartyOrderNo ?? formData.ThirdPartyOrderNo;
		formData.PromotionPlayerId = promotionPlayerId ?? formData.PromotionPlayerId;
		formData.UpdatedOn = DateTime.UtcNow;

		_ = await _tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask SubmitWithdrawFormStatusAsync(
		Guid formId,
		PaymentFlowState? status,
		string? paymentReturn,
		decimal? actualAmount,
		string? thirdPartyOrderNo,
		string? paidUserId,
		string? paidUserName,
		CancellationToken cancellationToken = default)
	{
		var formData = await _tgsContext.WithdrawForms
			.FindAsync(new object?[] { formId }, cancellationToken)
			.ConfigureAwait(false);

		var currentTime = DateTime.UtcNow;

		if (formData == null)
			return;

		formData.Status = status.HasValue ? (int)status.Value : formData.Status;
		formData.PaymentReturn = paymentReturn ?? formData.PaymentReturn;
		formData.ActualAmount = actualAmount ?? formData.ActualAmount;
		formData.ThirdPartyOrderNo = thirdPartyOrderNo ?? formData.ThirdPartyOrderNo;
		formData.PaidUserId = paidUserId ?? formData.PaidUserId;
		formData.PaidUserName = paidUserName ?? formData.PaidUserName;
		formData.UpdatedOn = currentTime;
		if (status == PaymentFlowState.Paid)
			formData.PaidOn = currentTime;

		_ = await _tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	private IQueryable<DepositForm> MakeDepositFormQuery(DepositFormQueryConditions conditions)
	{
		var dataQuery = _tgsContext.DepositForms.AsQueryable()
			.Include(f => f.PlayerCodeNavigation)
			.Include(f => f.BrandPayment)
			.ThenInclude(bp => bp.PaymentMethodChannel)
			.Where(form => form.BrandId == conditions.BrandId);

		if (!string.IsNullOrWhiteSpace(conditions.UserId))
			dataQuery = dataQuery.Where(form => form.PlayerCode == conditions.UserId);
		if (!string.IsNullOrWhiteSpace(conditions.UserName))
			dataQuery = dataQuery.Where(form => form.PlayerCodeNavigation.Username == conditions.UserName);
		if (!string.IsNullOrWhiteSpace(conditions.ChannelName))
			dataQuery = dataQuery.Where(form => form.BrandPayment.ChannelName == conditions.ChannelName);
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
		var dataQuery = _tgsContext.WithdrawForms.AsQueryable()
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

		if (conditions.ReviewOnRange != null)
			dataQuery = dataQuery.Where(form => form.ReviewedOn >= conditions.ReviewOnRange.Value.Begin
				&& form.ReviewedOn < conditions.ReviewOnRange.Value.End);

		if (conditions.PaidOnRange != null)
			dataQuery = dataQuery.Where(form => form.PaidOn >= conditions.PaidOnRange.Value.Begin
				&& form.PaidOn < conditions.PaidOnRange.Value.End);

		return dataQuery;
	}
}
