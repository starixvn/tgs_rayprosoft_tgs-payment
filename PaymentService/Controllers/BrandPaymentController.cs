using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices.BrandPayments;
using Tgs.Payment.ViewModels;
using TGS.ServiceUtility;

namespace Tgs.Payment.Controllers
{
	[Route("brand/payment")]
    [ApiController]
    public class BrandPaymentController : ControllerBase
    {
        [HttpGet("acl/{playerCode}")]
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

        [HttpGet("{id}")]
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

        [HttpGet]
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

        [HttpGet("vip")]
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
}