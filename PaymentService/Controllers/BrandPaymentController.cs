using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices;
using Tgs.Payment.ViewModels;
using TGS.ServiceUtility;

namespace Tgs.Payment.Controllers
{
	[Route("brand/payment")]
	[ApiController]
	public class BrandPaymentController : ControllerBase
	{
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
	}
}
