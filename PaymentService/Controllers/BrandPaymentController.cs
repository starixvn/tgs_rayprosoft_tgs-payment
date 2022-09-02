using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices.BrandPayments;
using Tgs.Payment.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Controllers;

[Route("brand/payment")]
[ApiController]
public class BrandPaymentController : ControllerBase
{
	[HttpGet("designatedusers/check", Name = "CheckBrandPaymentDesignatedUsersExist")]
	public IAsyncEnumerable<BrandPaymentPlayerCheckInfoViewModel> CheckBrandPaymentDesignatedUsersExistAsync(
		[FromServices] IQueryService<BrandPaymentDesignatedUsersExistQuery, BrandPaymentPlayerExistInfo> queryService,
		[FromQuery(Name = "brandPayment")] IEnumerable<Guid> brandPaymentIds)
		=> queryService
			.QueryAsync(new BrandPaymentDesignatedUsersExistQuery(brandPaymentIds))
			.Select(bpPlayerExistInfo => new BrandPaymentPlayerCheckInfoViewModel
			{
				BrandPaymentId = bpPlayerExistInfo.BrandPaymentId,
				HasDesignatedUsers = bpPlayerExistInfo.HasDesignatedUsers,
			});

	[HttpGet("acl/{playerCode}", Name = "QueryBrandPaymentAcls")]
	public IAsyncEnumerable<BrandPaymentAclViewModel> GetBrandPaymentAclsAsync(
		[FromServices] IQueryService<BrandPaymentAclQuery, BrandPaymentAcl> queryService,
		string playerCode,
		[FromQuery(Name = "brand")] IEnumerable<Guid> brandPaymentIds)
		=> queryService
			.QueryAsync(new BrandPaymentAclQuery(playerCode, brandPaymentIds))
			.Select(acl => new BrandPaymentAclViewModel
			{
				BrandPaymentId = acl.BrandPaymentId,
				IsBlock = acl.IsBlock,
				PlayerCode = acl.PlayerCode
			});

	[HttpGet("{id}", Name = "GetBrandPayment")]
	public async ValueTask<BrandPaymentViewModel> GetBrandPaymentAsync(
		[FromServices] IGetService<GetBrandPaymentQuery, BrandPaymentInfo> getService,
		Guid id)
	{
		var brandPayment = await getService
			.GetAsync(new GetBrandPaymentQuery(id))
			.ConfigureAwait(false);

		return new BrandPaymentViewModel
		{
			Id = brandPayment.Id,
			AuthenticateIdentity = brandPayment.AuthenticateIdentity,
			AuthenticatePhone = brandPayment.AuthenticatePhone,
			ChannelCode = brandPayment.ChannelCode,
			ChannelId = brandPayment.ChannelId,
			ChannelName = brandPayment.ChannelName,
			ConditionMatchType = brandPayment.ConditionMatchType,
			DepositedAmountLimit = brandPayment.DepositedAmountLimit,
			Enabled = brandPayment.Enabled,
			ExtendedData = brandPayment.ExtendedData,
			IsAllPlayers = brandPayment.IsAllPlayers,
			MethodId = brandPayment.MethodId,
			PaymentId = brandPayment.PaymentId,
			PaymentMethodChannelId = brandPayment.PaymentMethodChannelId,
			TransferLimit = brandPayment.TransferLimit,
			TransferType = brandPayment.TransferType,
		};
	}

	[HttpGet("designatedusers/{id}", Name = "GetBrandPaymentDesignatedUsers")]
	public IAsyncEnumerable<BrandPaymentPlayerInfoViewModel> GetBrandPaymentDesignatedUsersAsync(
		[FromServices] IQueryService<BrandPaymentDesignatedUserQuery, BrandPaymentPlayerInfo> queryService,
		Guid id)
		=> queryService
			.QueryAsync(new BrandPaymentDesignatedUserQuery(id))
			.Select(bppi => new BrandPaymentPlayerInfoViewModel
			{
				Username = bppi.Username
			});

	[HttpGet("fee/{id}", Name = "GetBrandPaymentFeeSettings")]
	public async ValueTask<BrandPaymentFeeViewModel> GetBrandPaymentFeeSettingsAsync(
		[FromServices] IGetService<GetBrandPaymentFeeQuery, BrandPaymentFeeInfo> getService,
		Guid id)
	{
		var brandPayment = await getService
			.GetAsync(new GetBrandPaymentFeeQuery(id))
			.ConfigureAwait(false);

		return new BrandPaymentFeeViewModel
		{
			HandlingFeeRateTenPercentile = brandPayment.HandlingFeeRateTenPercentile,
			ReducePlayerAmount = brandPayment.ReducePlayerAmount
		};
	}

	[HttpGet(Name = "GetBrandPayments")]
	public IAsyncEnumerable<BrandPaymentViewModel> GetBrandPaymentsAsync(
		[FromServices] IQueryService<BrandPaymentQuery, BrandPaymentInfo> queryService,
		[FromQuery(Name = "brand")] Guid brandId)
		=> queryService.QueryAsync(new BrandPaymentQuery(brandId))
			.Select(brandPayment => new BrandPaymentViewModel
			{
				Id = brandPayment.Id,
				AuthenticateIdentity = brandPayment.AuthenticateIdentity,
				AuthenticatePhone = brandPayment.AuthenticatePhone,
				ChannelCode = brandPayment.ChannelCode,
				ChannelId = brandPayment.ChannelId,
				ChannelName = brandPayment.ChannelName,
				ConditionMatchType = brandPayment.ConditionMatchType,
				DepositedAmountLimit = brandPayment.DepositedAmountLimit,
				Enabled = brandPayment.Enabled,
				ExtendedData = brandPayment.ExtendedData,
				IsAllPlayers = brandPayment.IsAllPlayers,
				MethodId = brandPayment.MethodId,
				PaymentId = brandPayment.PaymentId,
				PaymentMethodChannelId = brandPayment.PaymentMethodChannelId,
				TransferLimit = brandPayment.TransferLimit,
				TransferType = brandPayment.TransferType,
				DisplayOrder = brandPayment.DisplayOrder
			});

	[HttpGet("vip", Name = "QueryBrandPaymentVips")]
	public IAsyncEnumerable<BrandPaymentVipViewModel> GetBrandPaymentVipsAsync(
		[FromServices] IQueryService<BrandPaymentVipQuery, BrandPaymentVip> queryService,
		[FromQuery(Name = "brand")] IEnumerable<Guid> brandPaymentIds)
		=> queryService.QueryAsync(new BrandPaymentVipQuery(brandPaymentIds))
			.Select(vip => new BrandPaymentVipViewModel
			{
				BrandPaymentId = vip.BrandPaymentId,
				VipLevelId = vip.VipLevelId
			});

	[HttpGet("query", Name = "QueryBrandPayments")]
	public IAsyncEnumerable<BrandPaymentViewModel> QueryBrandPaymentsAsync(
		[FromServices] IQueryService<BrandPaymentsQuery, BrandPaymentInfo> queryService,
		[FromQuery(Name = "brand_id")] Guid? brandId,
		[FromQuery(Name = "transfer_type")] TransferType? transferType,
		[FromQuery(Name = "enabled")] bool? enabled,
		[FromQuery(Name = "keyword")] string? keyword)
	{
		return queryService.QueryAsync(new BrandPaymentsQuery(brandId, transferType, enabled, keyword))
				.Select(brandPayment => new BrandPaymentViewModel
				{
					Id = brandPayment.Id,
					AuthenticateIdentity = brandPayment.AuthenticateIdentity,
					AuthenticatePhone = brandPayment.AuthenticatePhone,
					ChannelCode = brandPayment.ChannelCode,
					ChannelId = brandPayment.ChannelId,
					ChannelName = brandPayment.ChannelName,
					ConditionMatchType = brandPayment.ConditionMatchType,
					DepositedAmountLimit = brandPayment.DepositedAmountLimit,
					Enabled = brandPayment.Enabled,
					ExtendedData = brandPayment.ExtendedData,
					IsAllPlayers = brandPayment.IsAllPlayers,
					MethodId = brandPayment.MethodId,
					PaymentId = brandPayment.PaymentId,
					PaymentMethodChannelId = brandPayment.PaymentMethodChannelId,
					TransferLimit = brandPayment.TransferLimit,
					TransferType = brandPayment.TransferType,
					DisplayOrder = brandPayment.DisplayOrder
				});
	}
}
