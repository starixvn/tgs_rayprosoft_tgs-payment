using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.Payment;
using Tgs.Payment.Repositories.Entities;
using TGS.Paging;

namespace TGS.Payment.Repositories
{
	internal class PaymentRepository : IPaymentRepository
	{
		private readonly TgsContext _tgsContext;

		public PaymentRepository(TgsContext tgsContext)
		{
			_tgsContext = tgsContext ?? throw new ArgumentNullException(nameof(tgsContext));
		}

		public ValueTask<int> CountDepositFormsAsync(DepositFormQueryConditions conditions)
		{
			var dataQuery = MakeDepositFormQueryAsync(conditions);

			return dataQuery.CountAsync();
		}

		public ValueTask<int> CountWithdrawFormsAsync(WithdrawFormQueryConditions conditions)
		{
			var dataQuery = MakeWithdrawFormQueryAsync(conditions);

			return dataQuery.CountAsync();
		}

		public async ValueTask CreateAgentBankInfoAsync(AgentBankInfo agentBankInfo, string modifyUser)
		{
			var createTime = DateTimeOffset.UtcNow;

			await _tgsContext.AgentBankInfoItems
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
				}).ConfigureAwait(false);

			await _tgsContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async ValueTask CreateBankInfoAsync(Tgs.Payment.PlayerBankInfo bankInfo, string modifyUser)
		{
			var createTime = DateTimeOffset.UtcNow;

			await _tgsContext.PlayerBankInfos
				.AddAsync(new Tgs.Payment.Repositories.Entities.PlayerBankInfo
				{
					Id = bankInfo.Id,
					PlayerCode = bankInfo.PlayerCode,
					BrandId = bankInfo.BrandId,
					PaymentChannelId = bankInfo.PaymentChannelId,
					BankType = bankInfo.BankType,
					Detail = bankInfo.Detail,
					CreatedByUsername = modifyUser,
					CreatedOn = createTime.UtcDateTime,
					UpdatedByUsername = modifyUser,
					UpdatedOn = createTime.UtcDateTime
				})
				.ConfigureAwait(false);

			await _tgsContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async ValueTask CreateDepositFormAsync(Guid createFormId, DepositFormCreateInfo form)
		{
			var currentDate = DateTimeOffset.UtcNow;

			await _tgsContext.DepositForms
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
					CreatedOn = currentDate.UtcDateTime,
					UpdatedOn = currentDate.UtcDateTime
				})
				.ConfigureAwait(false);

			await _tgsContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async ValueTask CreatePaymentAsync(PaymentCreateInfo createInfo)
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

			await _tgsContext.PaymentItems
				.AddAsync(createPayment)
				.ConfigureAwait(false);

			await _tgsContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async ValueTask CreatePaymentChannelAsync(string channelCode)
		{
			var createChannel = new PaymentChannel
			{
				Id = Guid.NewGuid(),
				Code = channelCode
			};

			await _tgsContext.PaymentChannels
				.AddAsync(createChannel)
				.ConfigureAwait(false);

			await _tgsContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async ValueTask CreatePaymentMethodAsync(string methodCode, string bankType)
		{
			var createMethod = new PaymentMethod
			{
				Id = Guid.NewGuid(),
				Code = methodCode,
				BankType = bankType
			};

			await _tgsContext.PaymentMethods
				.AddAsync(createMethod)
				.ConfigureAwait(false);

			await _tgsContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async ValueTask CreateWithdrawFormAsync(Guid createFormId, WithdrawFormCreateInfo form)
		{
			var currentDate = DateTimeOffset.UtcNow;

			await _tgsContext.WithdrawForms
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
					CreatedOn = currentDate.UtcDateTime,
					UpdatedOn = currentDate.UtcDateTime
				})
				.ConfigureAwait(false);

			await _tgsContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async ValueTask DeleteAgentBankInfoAsync(Guid agentId, Guid bankInfoId)
		{
			var bankInfo = await _tgsContext.AgentBankInfoItems
				.FindAsync(bankInfoId)
				.ConfigureAwait(false);

			if (bankInfo.AgentId == agentId)
			{
				_tgsContext.AgentBankInfoItems.Remove(bankInfo);

				await _tgsContext.SaveChangesAsync().ConfigureAwait(false);
			}
		}

		public async ValueTask DeleteBankInfoAsync(string playerCode, Guid bankInfoId)
		{
			var bankInfo = await _tgsContext.PlayerBankInfos
				.FindAsync(bankInfoId)
				.ConfigureAwait(false);

			if (bankInfo.PlayerCode == playerCode)
			{
				_tgsContext.PlayerBankInfos.Remove(bankInfo);

				await _tgsContext.SaveChangesAsync().ConfigureAwait(false);
			}
		}

		public async ValueTask<Tgs.Payment.AgentBankInfo> GetAgentBankInfoByIdAsync(Guid id)
		{
			var bankInfo = await _tgsContext.AgentBankInfoItems
				.FindAsync(id)
				.ConfigureAwait(false);

			return new Tgs.Payment.AgentBankInfo(
				bankInfo.Id,
				bankInfo.AgentId,
				bankInfo.BrandId,
				bankInfo.PaymentChannelId,
				bankInfo.BankType,
				bankInfo.Detail!);
		}

		public IAsyncEnumerable<Tgs.Payment.AgentBankInfo> GetAgentBankInfosByAgentIdAsync(Guid agentId)
			=> _tgsContext.AgentBankInfoItems.AsAsyncEnumerable()
				.Where(info => info.AgentId == agentId)
				.Select(info => new AgentBankInfo(
					info.Id,
					info.AgentId,
					info.BrandId,
					info.PaymentChannelId,
					info.BankType,
					info.Detail!));

		public IAsyncEnumerable<PaymentChannelInfo> GetAllPaymentChannelsAsync()
			=> _tgsContext.PaymentChannels.AsAsyncEnumerable()
				.Select(row => new PaymentChannelInfo(row.Id, row.Code));

		public IAsyncEnumerable<PaymentMethodChannelInfo> GetAllPaymentMethodChannelsAsync()
			=> _tgsContext.PaymentMethodChannels.AsAsyncEnumerable()
				.Select(row => new PaymentMethodChannelInfo(
					row.Id,
					row.PaymentId,
					row.MethodId,
					row.ChannelId));

		public IAsyncEnumerable<PaymentMethodInfo> GetAllPaymentMethodsAsync()
			=> _tgsContext.PaymentMethods.AsAsyncEnumerable()
				.Select(row => new PaymentMethodInfo(row.Id, row.Code, row.BankType));

		public IAsyncEnumerable<PaymentInfo> GetAllPaymentsAsync()
			=> _tgsContext.PaymentItems.AsAsyncEnumerable()
				.Select(row => new PaymentInfo(row.Id, row.Code));

		public async ValueTask<Tgs.Payment.PlayerBankInfo> GetBankInfoByIdAsync(Guid id)
		{
			var bankInfo = await _tgsContext.PlayerBankInfos
				.FindAsync(id)
				.ConfigureAwait(false);

			return new Tgs.Payment.PlayerBankInfo(
				bankInfo.Id,
				bankInfo.PlayerCode,
				bankInfo.BrandId,
				bankInfo.PaymentChannelId,
				bankInfo.BankType,
				bankInfo.Detail!);
		}

		public IAsyncEnumerable<Tgs.Payment.PlayerBankInfo> GetBankInfosByPlayerCodeAsync(string playerCode)
			=> _tgsContext.PlayerBankInfos.AsAsyncEnumerable()
				.Where(info => info.PlayerCode == playerCode)
				.Select(info => new Tgs.Payment.PlayerBankInfo(
					info.Id,
					info.PlayerCode,
					info.BrandId,
					info.PaymentChannelId,
					info.BankType,
					info.Detail!));

		public IAsyncEnumerable<BrandPaymentAcl> GetBrandPaymentAclsAsync(IEnumerable<Guid> brandPaymentIds, string playerCode)
		{
			var ids = brandPaymentIds.ToArray();

			var query = from acl in _tgsContext.BrandPaymentPlayers.AsAsyncEnumerable()
						where acl.PlayerCode == playerCode
							&& ids.Contains(acl.BrandPaymentId)
						select acl;

			return query.Select(acl => new BrandPaymentAcl(
				acl.BrandPaymentId,
				acl.PlayerCode,
				acl.ListType == 1));
		}

		public async ValueTask<BrandPaymentInfo> GetBrandPaymentByIdAsync(Guid id)
		{
			var row = await _tgsContext.BrandPayments
				.Include(bp => bp.PaymentMethodChannel)
				.ThenInclude(pmc => pmc.Channel)
				.Where(bp => bp.Id == id)
				.SingleAsync()
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

		public IAsyncEnumerable<BrandPaymentInfo> GetBrandPaymentsAsync(Guid brandId)
		{
			var query = from bp in _tgsContext.BrandPayments
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

		public IAsyncEnumerable<BrandPaymentVip> GetBrandPaymentVipListAsync(IEnumerable<Guid> brandPaymentIds)
		{
			var ids = brandPaymentIds.ToArray();

			return _tgsContext.BrandPaymentVipLevels.AsAsyncEnumerable()
				.Where(v => ids.Contains(v.BrandPaymentId))
				.Select(v => new BrandPaymentVip(v.BrandPaymentId, v.VipLevelId));
		}

		public ValueTask<DepositFormInfo> GetDepositFormByIdAsync(Guid id)
		{
			var query = from df in _tgsContext.DepositForms
							.Include(f => f.PlayerCodeNavigation)
							.Include(f => f.BrandPayment)
							.ThenInclude(bp => bp.PaymentMethodChannel)
							.AsAsyncEnumerable()
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
							df.Remark,
							(PaymentFlowState)df.Status,
							df.ApplyInfo,
							df.PaymentReturn,
							df.ThirdPartyOrderNo,
							df.PromotionId,
							df.CreatedOn,
							df.UpdatedOn);

			return query.SingleAsync();
		}

		public async ValueTask<PaymentInfo> GetPaymentAsync(Guid id)
		{
			var payment = await _tgsContext.PaymentItems
				.FindAsync(id)
				.ConfigureAwait(false);

			return new PaymentInfo(
				payment.Id,
				payment.Code);
		}

		public IAsyncEnumerable<PaymentMethodChannelInfo> GetPaymentMethodChannelsAsync()
		{
			return _tgsContext.PaymentMethodChannels.ToAsyncEnumerable()
				.Select(pmc => new PaymentMethodChannelInfo(pmc.Id, pmc.PaymentId, pmc.MethodId, pmc.ChannelId));
		}

		public ValueTask<WithdrawFormInfo> GetWithdrawFormByIdAsync(Guid formId)
		{
			var query = from df in _tgsContext.WithdrawForms
							.Include(f => f.BrandPayment!)
							.ThenInclude(bp => bp.PaymentMethodChannel)
							.AsAsyncEnumerable()
						where df.Id == formId
						select new WithdrawFormInfo(
							df.Id,
							df.UserId,
							(UserType)df.UserType,
							df.UserName,
							df.BrandId,
							df.Amount,
							(PaymentFlowState)df.Status,
							df.Remark,
							df.BankInfoId,
							df.CreatedOn,
							df.UpdatedOn,
							df.BrandPaymentId,
							df.BrandPayment?.ChannelName,
							df.BrandPayment?.PaymentMethodChannel?.MethodId,
							df.BankInfo,
							df.ApplyInfo,
							df.PaymentReturn,
							df.ThirdPartyOrderNo);

			return query.SingleAsync();
		}

		public IAsyncEnumerable<DepositFormInfo> QueryDepositFormsAsync(
			DepositFormQueryConditions conditions,
			Paging<DepositFormInfo> paging)
		{
			var dataQuery = MakeDepositFormQueryAsync(conditions)
				.Select(form => new DepositFormInfo(
					form.Id,
					form.PlayerCode,
					form.PlayerCodeNavigation.Username,
					form.BrandId,
					form.BrandPaymentId,
					form.BrandPayment.ChannelName,
					form.BrandPayment.PaymentMethodChannel.MethodId,
					form.Amount,
					form.Remark,
					(PaymentFlowState)form.Status,
					form.ApplyInfo,
					form.PaymentReturn,
					form.ThirdPartyOrderNo,
					form.PromotionId,
					form.CreatedOn,
					form.UpdatedOn));

			dataQuery = dataQuery.ApplyOrderBy(paging).ApplyPaging(paging);

			return dataQuery
				.ToEnumerable()
				.ToAsyncEnumerable();
		}

		public IAsyncEnumerable<DepositFormInfo> QueryDepositFormsAsync(DepositFormQueryConditions conditions)
		{
			var dataQuery = MakeDepositFormQueryAsync(conditions);

			return dataQuery
				.Select(form => new DepositFormInfo(
					form.Id,
					form.PlayerCode,
					form.PlayerCodeNavigation.Username,
					form.BrandId,
					form.BrandPaymentId,
					form.BrandPayment.ChannelName,
					form.BrandPayment.PaymentMethodChannel.MethodId,
					form.Amount,
					form.Remark,
					(PaymentFlowState)form.Status,
					form.ApplyInfo,
					form.PaymentReturn,
					form.ThirdPartyOrderNo,
					form.PromotionId,
					form.CreatedOn,
					form.UpdatedOn))
				.ToEnumerable()
				.ToAsyncEnumerable();
		}

		public IAsyncEnumerable<WithdrawFormInfo> QueryWithdrawFormsAsync(
			WithdrawFormQueryConditions conditions,
			Paging<WithdrawFormInfo> paging)
		{
			var dataQuery = MakeWithdrawFormQueryAsync(conditions)
				.Select(form => new WithdrawFormInfo(
					form.Id,
					form.UserId,
					(UserType)form.UserType,
					form.UserName,
					form.BrandId,
					form.ActualAmount ?? form.Amount,
					(PaymentFlowState)form.Status,
					form.Remark,
					form.BankInfoId,
					form.CreatedOn,
					form.UpdatedOn,
					form.BrandPaymentId,
					form.BrandPayment?.ChannelName,
					form.BrandPayment?.PaymentMethodChannel.MethodId,
					form.BankInfo,
					form.ApplyInfo,
					form.PaymentReturn,
					form.ThirdPartyOrderNo));

			dataQuery = dataQuery.ApplyOrderBy(paging).ApplyPaging(paging);

			return dataQuery
				.ToEnumerable()
				.ToAsyncEnumerable();
		}

		public IAsyncEnumerable<WithdrawFormInfo> QueryWithdrawFormsAsync(WithdrawFormQueryConditions conditions)
		{
			var dataQuery = MakeWithdrawFormQueryAsync(conditions);

			return dataQuery
				.Select(form => new WithdrawFormInfo(
					form.Id,
					form.UserId,
					(UserType)form.UserType,
					form.UserName,
					form.BrandId,
					form.ActualAmount ?? form.Amount,
					(PaymentFlowState)form.Status,
					form.Remark,
					form.BankInfoId,
					form.CreatedOn,
					form.UpdatedOn,
					form.BrandPaymentId,
					form.BrandPayment?.ChannelName,
					form.BrandPayment?.PaymentMethodChannel.MethodId,
					form.BankInfo,
					form.ApplyInfo,
					form.PaymentReturn,
					form.ThirdPartyOrderNo))
				.ToEnumerable()
				.ToAsyncEnumerable();
		}

		public async ValueTask SignOffDepositFormAsync(Guid formId, ReviewState reviewStatus, string? remark = null)
		{
			var formData = await _tgsContext.DepositForms
				.FindAsync(formId)
				.ConfigureAwait(false);

			formData.Status = reviewStatus.ToNumber();
			formData.Remark = remark;
			formData.UpdatedOn = DateTime.UtcNow;

			await _tgsContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async ValueTask SignOffWithdrawFormAsync(
			Guid formId,
			Guid? brandPaymentId,
			string? applyInfo,
			ReviewState reviewStatus,
			string? remark)
		{
			var formData = await _tgsContext.WithdrawForms
				.FindAsync(formId)
				.ConfigureAwait(false);

			formData.BrandPaymentId = brandPaymentId;
			formData.ApplyInfo = applyInfo;
			formData.Status = reviewStatus.ToNumber();
			formData.Remark = remark;
			formData.UpdatedOn = DateTime.UtcNow;

			await _tgsContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async ValueTask SubmitDepositFormStatusAsync(Guid formId, PaymentFlowState status, string? paymentReturn)
		{
			var formData = await _tgsContext.DepositForms.FindAsync(formId).ConfigureAwait(false);

			formData.Status = (int)status;
			formData.PaymentReturn = paymentReturn;

			await _tgsContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async ValueTask SubmitDepositFormThirdPartyOrderNoAsync(Guid formId, string? thirdPartyOrderNo)
		{
			var formData = await _tgsContext.DepositForms.FindAsync(formId).ConfigureAwait(false);

			formData.ThirdPartyOrderNo = thirdPartyOrderNo;

			await _tgsContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async ValueTask SubmitWithdrawFormStatusAsync(Guid formId, PaymentFlowState status, string? paymentReturn, decimal? actualAmount)
		{
			var formData = await _tgsContext.WithdrawForms.FindAsync(formId).ConfigureAwait(false);

			formData.Status = (int)status;
			formData.PaymentReturn = paymentReturn;
			formData.ActualAmount = actualAmount;

			await _tgsContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async ValueTask SubmitWithdrawFormThirdPartyOrderNoAsync(Guid formId, string? ThirdPartyOrderNo)
		{
			var formData = await _tgsContext.WithdrawForms.FindAsync(formId).ConfigureAwait(false);

			formData.ThirdPartyOrderNo = ThirdPartyOrderNo;

			await _tgsContext.SaveChangesAsync().ConfigureAwait(false);
		}

		private IAsyncEnumerable<DepositForm> MakeDepositFormQueryAsync(DepositFormQueryConditions conditions)
		{
			var dataQuery = _tgsContext.DepositForms
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

			return dataQuery.AsAsyncEnumerable();
		}

		private IAsyncEnumerable<WithdrawForm> MakeWithdrawFormQueryAsync(WithdrawFormQueryConditions conditions)
		{
			var dataQuery = _tgsContext.WithdrawForms
				.Include(f => f.BrandPayment)
				.ThenInclude(bp => bp!.PaymentMethodChannel)
				.Where(form => form.BrandId == conditions.BrandId);

			if (!string.IsNullOrWhiteSpace(conditions.UserId))
				dataQuery = dataQuery.Where(form => form.UserId == conditions.UserId);

			if (conditions.UserType != null)
				dataQuery = dataQuery.Where(form => form.UserType == (int)conditions.UserType);

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

			return dataQuery.AsAsyncEnumerable();
		}
	}
}
