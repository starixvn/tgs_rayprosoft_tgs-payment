using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Tgs.Payment.BrandPayment.PaymentConditions;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class BrandPaymentsQueryService : IQueryService<BrandPaymentsQuery, BrandPaymentInfo>
{
	private readonly IQueryService<BrandPaymentAclQuery, BrandPaymentAcl> _aclQueryService;
	private readonly IQueryService<BrandPaymentQuery, BrandPaymentInfo> _queryService;
	private readonly IQueryService<BrandPaymentVipQuery, BrandPaymentVip> _vipQueryService;

	public BrandPaymentsQueryService(
		IQueryService<BrandPaymentQuery, BrandPaymentInfo> queryService,
		IQueryService<BrandPaymentAclQuery, BrandPaymentAcl> aclQueryService,
		IQueryService<BrandPaymentVipQuery, BrandPaymentVip> vipQueryService)
	{
		_queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
		_aclQueryService = aclQueryService ?? throw new ArgumentNullException(nameof(aclQueryService));
		_vipQueryService = vipQueryService ?? throw new ArgumentNullException(nameof(vipQueryService));
	}

	public async IAsyncEnumerable<BrandPaymentInfo> QueryAsync(
		BrandPaymentsQuery query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var brandPayments = await _queryService
			.QueryAsync(new BrandPaymentQuery(query.BrandId))
			.ToArrayAsync()
			.ConfigureAwait(false)!;

		var acls = await _aclQueryService
			.QueryAsync(new BrandPaymentAclQuery(query.PlayerCode, brandPayments.Select(bp => bp.Id)))
			.ToArrayAsync()
			.ConfigureAwait(false);

		var blockList = acls
			.Where(acl => acl.IsBlock)
			.Select(acl => acl.BrandPaymentId)
			.Distinct()
			.ToArray();

		var whiteList = acls
			.Where(acl => !acl.IsBlock)
			.Select(acl => acl.BrandPaymentId)
			.Distinct()
			.ToArray();

		var brandPaymentVipLevels = await _vipQueryService
			.QueryAsync(new BrandPaymentVipQuery(brandPayments.Select(bp => bp.Id)))
			.ToLookupAsync(bv => bv.BrandPaymentId, bv => bv.VipLevelId)
			.ConfigureAwait(false);

		var playerInfo = new PlayerInfo
		{
			BrandId = query.BrandId,
			PlayerCode = query.PlayerCode,
			DepositedAmount = query.DepositedAmount,
			IdentityAuthentication = query.IdentityAuthentication,
			PhoneAuthentication = query.PhoneAuthentication,
			VipLevelId = query.VipLevelId
		};

		var blockListCondition = new BlockListCondition(blockList);
		var whiteListCondition = new WhiteListCondition(whiteList);
		var showOnAllPlayerCondition = new ShowOnAllPlayersCondition();

		foreach (var bp in brandPayments)
		{
			if (!bp.Enabled)
				continue;

			var depositedAmountCondition = new DepositedAmountCondition(bp.DepositedAmountLimit);
			var identityCondition = new IdentityCondition(bp.AuthenticateIdentity);
			var phoneCondition = new PhoneCondition(bp.AuthenticatePhone);
			var vipCondition = new VipCondition(brandPaymentVipLevels.Contains(bp.Id)
					? brandPaymentVipLevels[bp.Id]
					: Array.Empty<Guid>());

			var filterCondition = new BrandPaymentFilterCondition(
				blockListCondition,
				whiteListCondition,
				showOnAllPlayerCondition,
				bp.ConditionMatchType == ConditionMatchType.MatchAll
					? new MatchAllCondition(
						depositedAmountCondition,
						identityCondition,
						phoneCondition,
						vipCondition)
					: new MatchOneCondition(
						depositedAmountCondition,
						identityCondition,
						phoneCondition,
						vipCondition));

			if (filterCondition.IsValid(bp, playerInfo!, query.DepositedAmount))
				yield return bp;
		}
	}
}
