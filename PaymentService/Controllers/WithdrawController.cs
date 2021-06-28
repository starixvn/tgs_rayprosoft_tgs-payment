using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.ViewModels;
using TGS.ServiceUtility;

namespace Tgs.Payment.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class WithdrawController : ControllerBase
	{
		[HttpPut]
		public async Task<Guid> CreateWithdrawFormAsync(
			[FromServices] ICommandService<CreateWithdrawFormCommand> createService,
			[FromBody] CreateWithdrawFormViewModel request)
		{
			var command = new CreateWithdrawFormCommand(
				request.UserId,
				request.UserName,
				request.UserType,
				request.BrandId,
				request.BankInfoId,
				request.Amount);

			await createService.ExecuteAsync(command).ConfigureAwait(false);

			return await command.ReturnFormIdSource.Task.ConfigureAwait(false);
		}

		[HttpGet("{brandId}/{formId}")]
		public async Task<WithdrawFormViewModel?> GetWithdrawFormAsync(
			[FromServices] IGetService<GetWithdrawFormDetailQuery, WithdrawFormInfo?> getService,
			Guid brandId,
			Guid formId)
		{
			var query = new GetWithdrawFormDetailQuery(formId, brandId);

			var formData = await getService.GetAsync(query).ConfigureAwait(false);

			if (formData != null)
				return new WithdrawFormViewModel
				{
					Id = formData.Id,
					Amount = formData.Amount,
					ApplyInfo = formData.ApplyInfo,
					BankInfo = formData.BankInfo,
					BankInfoId = formData.BankInfoId,
					BrandId = formData.BrandId,
					BrandPaymentId = formData.BrandPaymentId,
					ChannelName = formData.ChannelName,
					CreatedOn = formData.CreatedOn,
					PaymentMethodId = formData.PaymentMethodId,
					PaymentReturn = formData.PaymentReturn,
					Remark = formData.Remark,
					Status = formData.Status,
					UpdatedOn = formData.UpdatedOn,
					UserId = formData.UserId,
					UserName = formData.UserName,
					UserType = formData.UserType
				};
			else
				return null;
		}

		[HttpPost("{formId}")]
		public async Task ReviewWithdrawFormAsync(
			[FromServices] ICommandService<SignOffWithdrawFormCommand> signOffService,
			Guid formId,
			[FromBody] SignOffWithdrawFormViewModel request)
		{
			var command = new SignOffWithdrawFormCommand(
				formId,
				request.BrandPaymentId,
				request.ReviewState,
				request.Remark);

			await signOffService.ExecuteAsync(command).ConfigureAwait(false);
		}

		[HttpPatch("{formId}")]
		public async Task SubmitWithdrawFormStatusAsync(
			[FromServices] ICommandService<SubmitWithdrawFormStatusCommand> submitService,
			Guid formId,
			[FromBody] WithdrawFormStatusViewModel request)
		{
			var command = new SubmitWithdrawFormStatusCommand(
				formId,
				request.Status,
				request.PaymentReturn,
				request.ActualAmount);

			await submitService.ExecuteAsync(command).ConfigureAwait(false);
		}
	}
}
