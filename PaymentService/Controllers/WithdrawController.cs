using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.Controllers.Constraints;
using Tgs.Payment.ViewModels;
using TGS.Paging;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class WithdrawController : ControllerBase
	{
		private readonly ILogger<WithdrawController> _logger;

		public WithdrawController(ILogger<WithdrawController> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		[HttpGet("untreated/{brandId}")]
		public ValueTask<int> CountUntreatedWithdrawFormsAsync(
			[FromServices] IGetService<WithdrawFormQuery, int> countService,
			Guid brandId)
		{
			var currentDate = DateTimeOffset.Now;

			var sourcePaging = new Paging<WithdrawFormInfo>();

			sourcePaging.Page = -1;
			sourcePaging.RowCount = -1;

			var query = new WithdrawFormQuery(
				brandId,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				new[] { PaymentFlowState.WaitApprove, PaymentFlowState.Init, PaymentFlowState.Approved },
				null,
				(Begin: currentDate.AddDays(-1D), End: currentDate),
				null,
				null,
				sourcePaging);

			return countService.GetAsync(query);
		}

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

			return formData != null
				? new WithdrawFormViewModel
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
					UserType = formData.UserType,
					ThirdPartyOrderNo = formData.ThirdPartyOrderNo,
					OrderNo = formData.OrderNo
				}
				: null;
		}

		[HttpGet("{formId}"), HttpHeaderConstraint("FormQuery")]
		public async Task<WithdrawFormViewModel?> GetWithdrawFormAsync(
			[FromServices] IGetService<GetWithdrawFormDetailByFormIdQuery, WithdrawFormInfo?> getService,
			Guid formId)
		{
			var query = new GetWithdrawFormDetailByFormIdQuery(formId);

			var formData = await getService.GetAsync(query).ConfigureAwait(false);

			return formData != null
				? new WithdrawFormViewModel
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
					UserType = formData.UserType,
					ThirdPartyOrderNo = formData.ThirdPartyOrderNo,
					ReviewedUserName = formData.ReviewedUserName,
					ReviewedOn = formData.ReviewedOn,
					OrderNo = formData.OrderNo,
					PaidOn = formData.PaidOn,
					PaidUserName = formData.PaidUserName,
					PaidUserId = formData.PaidUserId
				}
				: null;
		}

		[HttpGet("{brandId}")]
		public async Task<PagingWithdrawFormViewModel> QueryWithdrawFormsAsync(
			[FromServices] IGetService<WithdrawFormQuery, int> countService,
			[FromServices] IQueryService<WithdrawFormQuery, WithdrawFormInfo> queryService,
			[FromServices] IGetService<WithdrawFormQuery, decimal> totalAmountService,
			Guid brandId,
			[FromQuery] WithdrawFormQueryViewModel formQuery,
			[FromQuery] PagingViewModel paging)
		{
			_logger.LogInformation($"Query WithdrawForms Request => {JsonSerializer.Serialize(formQuery)}");

			var createOnRange = ((DateTimeOffset Begin, DateTimeOffset End)?)null;
			var updateOnRange = ((DateTimeOffset Begin, DateTimeOffset End)?)null;
			var reviewOnRange = ((DateTimeOffset Begin, DateTimeOffset End)?)null;
			var paidOnRange = ((DateTimeOffset Begin, DateTimeOffset End)?)null;

			if (formQuery.BeginCreatedOn != null && formQuery.EndCreatedOn != null)
				createOnRange = (formQuery.BeginCreatedOn.Value, formQuery.EndCreatedOn.Value);

			if (formQuery.BeginUpdateOn != null && formQuery.EndUpdatedOn != null)
				updateOnRange = (formQuery.BeginUpdateOn.Value, formQuery.EndUpdatedOn.Value);

			if (formQuery.BeginReviewOn != null && formQuery.EndReviewOn != null)
				reviewOnRange = (formQuery.BeginReviewOn.Value, formQuery.EndReviewOn.Value);

			if (formQuery.BeginPaidOn != null && formQuery.EndPaidOn != null)
				paidOnRange = (formQuery.BeginPaidOn.Value, formQuery.EndPaidOn.Value);

			var sourcePaging = new Paging<WithdrawFormInfo>();

			sourcePaging.Page = paging.Page ?? 1;
			sourcePaging.RowCount = paging.Rows ?? 10;

			if (paging.ShowAll == true)
			{
				sourcePaging.Page = -1;
				sourcePaging.RowCount = -1;
			}

			if (!string.IsNullOrWhiteSpace(paging.SortBy))
				sourcePaging.SortByPropertyName(paging.SortBy, paging.IsDesc ?? false);

			var query = new WithdrawFormQuery(
				brandId,
				formQuery.UserId,
				formQuery.UserName,
				formQuery.UserType,
				formQuery.ChannelName,
				formQuery.ThirdPartyOrderNo,
				formQuery.OrderNo,
				formQuery.FormId,
				formQuery.PaymentMethodId,
				formQuery.Status ?? Array.Empty<PaymentFlowState>(),
				createOnRange,
				updateOnRange,
				paidOnRange,
				reviewOnRange,
				sourcePaging);

			var totalCount = await countService.GetAsync(query).ConfigureAwait(false);
			var sum = await totalAmountService.GetAsync(query).ConfigureAwait(false);

			var items = queryService.QueryAsync(query)
				.Select(row => new WithdrawFormViewModel
				{
					Id = row.Id,
					Amount = row.Amount,
					ApplyInfo = row.ApplyInfo,
					BrandId = row.BrandId,
					BrandPaymentId = row.BrandPaymentId,
					ChannelName = row.ChannelName,
					CreatedOn = row.CreatedOn,
					PaymentMethodId = row.PaymentMethodId,
					PaymentReturn = row.PaymentReturn,
					UserId = row.UserId,
					UserType = row.UserType,
					Remark = row.Remark,
					Status = row.Status,
					UpdatedOn = row.UpdatedOn,
					UserName = row.UserName,
					ThirdPartyOrderNo = row.ThirdPartyOrderNo,
					ReviewedUserName = row.ReviewedUserName,
					ReviewedOn = row.ReviewedOn,
					OrderNo = row.OrderNo,
					PaidUserId = row.PaidUserId,
					PaidUserName = row.PaidUserName,
					PaidOn = row.PaidOn
				});

			return new PagingWithdrawFormViewModel
			{
				TotalCount = totalCount,
				Page = 1,
				Rows = -1,
				Items = items.ToEnumerable(),
				Sum = sum
			};
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
				request.ReviewUserId,
				request.ReviewUserName,
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
				request.ActualAmount,
				request.ThirdPartyOrderNo,
				request.PaidUserId,
				request.PaidUserName);

			await submitService.ExecuteAsync(command).ConfigureAwait(false);
		}
	}
}
