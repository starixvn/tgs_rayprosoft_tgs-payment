using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices.BrandPayments;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.Payment.Extensions;
using Tgs.Payment.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Controllers;

[Route("brand/payment")]
[ApiController]
public class BrandPaymentController : ControllerBase
{
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
			TransferType = brandPayment.TransferType
		};
	}

	[HttpGet("player", Name = "GetBrandPaymentsByPlayerInfo")]
	public async IAsyncEnumerable<BrandPaymentResponse> GetBrandPaymentsByPlayerInfoAsync(
		[FromServices] IQueryService<BrandPaymentsQuery, BrandPaymentInfo> brandPaymentsQueryService,		
		[FromServices] IQueryService<AllPaymentMethodsQuery, PaymentMethodInfo> paymentMethodService,
		[FromServices] IGetService<GetPaymentQuery, PaymentInfo> getPaymentService,
		[FromQuery(Name = "brandId")] Guid brandId,
		[FromQuery(Name = "playerCode")] string playerCode,
		[FromQuery(Name = "phoneAuthentication")] bool phoneAuthentication,
		[FromQuery(Name = "identityAuthentication")] bool identityAuthentication,
		[FromQuery(Name = "depositedAmount")] decimal depositedAmount,
		[FromQuery(Name = "vipLevelId")] Guid? vipLevelId)		
	{		
		var paymentMethods = new ReadOnlyDictionary<Guid, PaymentMethodInfo>(
			await paymentMethodService.QueryAsync(new AllPaymentMethodsQuery())
			.ToDictionaryAsync(method => method.Id)
			.ConfigureAwait(false));
		

		var query = new BrandPaymentsQuery(brandId, playerCode,identityAuthentication,phoneAuthentication,vipLevelId,depositedAmount);

		var brandPayments = await brandPaymentsQueryService.QueryAsync(query)
			.ToArrayAsync()
			.ConfigureAwait(false);

		var brandPaymentQuery = await brandPayments
			.Where(bp => bp.TransferType == TransferType.Deposit)
			.AsParallel()
			.WithCancellation(HttpContext.RequestAborted)
			.Select(async bp =>
			{
				var getPaymentQuery = new GetPaymentQuery(bp.PaymentId);

				var paymentInfo = await getPaymentService
					.GetAsync(getPaymentQuery)
					.ConfigureAwait(false);

				var paymentMethod = paymentMethods[bp.MethodId];

				return new BrandPaymentResponse
				{
					Id = bp.Id,
					ChannelId = bp.ChannelId,
					ChannelName = bp.ChannelName,
					ChannelCode = bp.ChannelCode,
					TransferLimit = new ValueRange<decimal>
					{
						From = bp.TransferLimit.From,
						To = bp.TransferLimit.To
					},
					ExtendedData = paymentInfo.Code == "Manual"
						? JsonDocument.Parse(bp.ExtendedData)
						: JsonDocument.Parse("{}"),
					PaymentId = bp.PaymentId,
					PaymentCode = paymentInfo.Code,
					MethodId = bp.MethodId,
					MethodCode = paymentMethod.Name,
					BankType = paymentMethod.BankType,
					DisplayOrder = bp.DisplayOrder
				};
			})
			.WhenAll()
			.ConfigureAwait(false);

		foreach (var bp in brandPaymentQuery)
		{
			yield return bp;
		}
	}
	

	[HttpGet(Name = "QueryBrandPayments")]
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
}
