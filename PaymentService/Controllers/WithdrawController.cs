using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices;
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
