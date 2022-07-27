using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices.Deposits;
using Tgs.Payment.Controllers.Constraints;
using Tgs.Payment.ViewModels;
using Tgs.ServiceUtility;
using TGS.Paging;

namespace Tgs.Payment.Controllers;

[Route("[controller]")]
[ApiController]
public class DepositController : ControllerBase
{
	/// <summary>
	/// Creates the deposit form asynchronous.
	/// </summary>
	/// <param name="createService">The create service.</param>
	/// <param name="createFormId">The create form identifier.</param>
	/// <param name="request">The request.</param>
	/// <returns></returns>
	[HttpPut("{createFormId}", Name = "CreateDepositForm")]
	public async Task<Guid> CreateDepositFormAsync(
		[FromServices] ICommandService<CreateDepositFormCommand> createService,
		Guid createFormId,
		[FromBody] CreateDepositFormViewModel request)
	{
		var createInfo = new DepositFormCreateInfo
		{
			PlayerCode = request.PlayerCode,
			BrandId = request.BrandId,
			BrandPaymentId = request.BrandPaymentId,
			Amount = request.Amount,
			PromotionId = request.PromotionId,
			ApplyInfo = request.ApplyInfo
		};

		var createCommand = new CreateDepositFormCommand(
			createFormId,
			createInfo);

		await createService.ExecuteAsync(createCommand).ConfigureAwait(false);

		return createCommand.FormId;
	}

	/// <summary>
	/// Gets the deposit form asynchronous.
	/// </summary>
	/// <param name="getService">The get service.</param>
	/// <param name="brandId">Brand id.</param>
	/// <param name="formId">Form id.</param>
	/// <returns></returns>
	[HttpGet("{brandId}/{formId}", Name = "GetDepositFormWithBrandId")]
	public async Task<DepositFormViewModel?> GetDepositFormAsync(
		[FromServices] IGetService<GetDepositFormDetailQuery, DepositFormInfo?> getService,
		Guid brandId,
		Guid formId)
	{
		var query = new GetDepositFormDetailQuery(formId, brandId);

		var formData = await getService.GetAsync(query).ConfigureAwait(false);

		return formData != null
			? new DepositFormViewModel
			{
				Id = formData.Id,
				ActualAmount = formData.ActualAmount,
				Amount = formData.Amount,
				ApplyInfo = formData.ApplyInfo,
				BrandId = formData.BrandId,
				BrandPaymentId = formData.BrandPaymentId,
				ChannelName = formData.ChannelName,
				CreatedOn = formData.CreatedOn,
				PaymentMethodId = formData.PaymentMethodId,
				PaymentReturn = formData.PaymentReturn,
				PlayerCode = formData.PlayerCode,
				PromotionId = formData.PromotionId,
				Remark = formData.Remark,
				Status = formData.Status,
				UpdatedOn = formData.UpdatedOn,
				UserName = formData.UserName,
				ThirdPartyOrderNo = formData.ThirdPartyOrderNo,
				OrderNo = formData.OrderNo,
				PromotionPlayerId = formData.PromotionPlayerId,
				ReviewedUserName = formData.ReviewedUserName,
				ReviewedOn = formData.ReviewedOn
			}
			: null;
	}

	[HttpGet("{formId}", Order = 1, Name = "GetDepositForm"), HttpHeaderConstraint("FormQuery")]
	public async Task<DepositFormViewModel?> GetDepositFormAsync(
		[FromServices] IGetService<GetDepositFormDetailByFormIdQuery, DepositFormInfo?> getService,
		Guid formId)
	{
		var query = new GetDepositFormDetailByFormIdQuery(formId);

		var formData = await getService.GetAsync(query).ConfigureAwait(false);

		return formData != null
			? new DepositFormViewModel
			{
				Id = formData.Id,
				Amount = formData.Amount,
				ActualAmount = formData.ActualAmount,
				ApplyInfo = formData.ApplyInfo,
				BrandId = formData.BrandId,
				BrandPaymentId = formData.BrandPaymentId,
				ChannelName = formData.ChannelName,
				CreatedOn = formData.CreatedOn,
				PaymentMethodId = formData.PaymentMethodId,
				PaymentReturn = formData.PaymentReturn,
				PlayerCode = formData.PlayerCode,
				PromotionId = formData.PromotionId,
				Remark = formData.Remark,
				Status = formData.Status,
				UpdatedOn = formData.UpdatedOn,
				UserName = formData.UserName,
				ThirdPartyOrderNo = formData.ThirdPartyOrderNo,
				OrderNo = formData.OrderNo,
				PromotionPlayerId = formData.PromotionPlayerId,
				ReviewedUserName = formData.ReviewedUserName,
				ReviewedOn = formData.ReviewedOn
			}
			: null;
	}

	/// <summary>
	/// Gets the untreated deposit form count asynchronous.
	/// </summary>
	/// <param name="countService">The count service.</param>
	/// <param name="brandId">The brand identifier.</param>
	/// <returns></returns>
	[HttpGet("untreated/{brandId}", Name = "CountUntreatedDepositForms")]
	public ValueTask<int> GetUntreatedDepositFormCountAsync(
		[FromServices] IGetService<DepositFormQuery, int> countService,
		Guid brandId)
	{
		var currentDate = DateTimeOffset.UtcNow;

		var sourcePaging = new Paging<DepositFormInfo>
		{
			Page = -1,
			RowCount = -1
		};

		var query = new DepositFormQuery(
			brandId,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			new[] {
				PaymentFlowState.WaitApprove,
				PaymentFlowState.WaitPayment
			},
			null,
			(Begin: currentDate.AddDays(-1D), End: currentDate),
			sourcePaging);

		return countService.GetAsync(query);
	}

	/// <summary>
	/// Locks the payment asynchronous.
	/// </summary>
	/// <param name="lockService">The lock service.</param>
	/// <param name="viewModel">The view model.</param>
	/// <returns></returns>
	[HttpPut("mutex", Name = "LockDepositPayment")]
	public ValueTask<Guid> LockPaymentAsync(
		[FromServices] IGetService<LockPaymentCommand, Guid> lockService,
		[FromBody] LockDepositPaymentViewModel viewModel)
	{
		var command = new LockPaymentCommand(
			viewModel.FormId,
			viewModel.PaymentCode,
			viewModel.PlayerCode);

		return lockService.GetAsync(command, HttpContext.RequestAborted);
	}

	/// <summary>
	/// Queries the deposit forms asynchronous.
	/// </summary>
	/// <param name="countService">The count service.</param>
	/// <param name="queryService">The query service.</param>
	/// <param name="totalAmountService">The total amount service.</param>
	/// <param name="brandId">The brand identifier.</param>
	/// <param name="formQuery">The form query.</param>
	/// <param name="paging">The paging.</param>
	/// <returns></returns>
	[HttpGet("{brandId}", Order = 10, Name = "QueryDepositForms")]
	public async Task<DepositViewModel> QueryDepositFormsAsync(
		[FromServices] IGetService<DepositFormQuery, int> countService,
		[FromServices] IQueryService<DepositFormQuery, DepositFormInfo> queryService,
		[FromServices] IGetService<DepositFormQuery, decimal> totalAmountService,
		Guid brandId,
		[FromQuery] DepositFormQueryViewModel formQuery,
		[FromQuery] PagingViewModel paging)
	{
		var createOnRange = ((DateTimeOffset Begin, DateTimeOffset End)?)null;
		var updateOnRange = ((DateTimeOffset Begin, DateTimeOffset End)?)null;

		if (formQuery.BeginCreatedOn != null && formQuery.EndCreatedOn != null)
			createOnRange = (formQuery.BeginCreatedOn.Value, formQuery.EndCreatedOn.Value);

		if (formQuery.BeginUpdateOn != null && formQuery.EndUpdatedOn != null)
			updateOnRange = (formQuery.BeginUpdateOn.Value, formQuery.EndUpdatedOn.Value);

		var sourcePaging = new Paging<DepositFormInfo>
		{
			Page = paging.Page ?? 1,
			RowCount = paging.Rows ?? 10
		};

		if (paging.ShowAll == true)
		{
			sourcePaging.Page = -1;
			sourcePaging.RowCount = -1;
		}

		if (!string.IsNullOrWhiteSpace(paging.SortBy))
			sourcePaging.SortByPropertyName(paging.SortBy, paging.IsDesc ?? false);

		var query = new DepositFormQuery(
			brandId,
			formQuery.UserId,
			formQuery.UserName,
			formQuery.ChannelName,
			formQuery.ThirdPartyOrderNo,
			formQuery.OrderNo,
			formQuery.FormId,
			formQuery.PaymentMethodId,
			formQuery.Status ?? Array.Empty<PaymentFlowState>(),
			createOnRange,
			updateOnRange,
			sourcePaging);

		var totalCount = await countService.GetAsync(query).ConfigureAwait(false);
		var sum = await totalAmountService.GetAsync(query).ConfigureAwait(false);

		var items = queryService.QueryAsync(query)
			.Select(row => new DepositFormViewModel
			{
				Id = row.Id,
				Amount = row.Amount,
				ActualAmount = row.ActualAmount,
				ApplyInfo = row.ApplyInfo,
				BrandId = row.BrandId,
				BrandPaymentId = row.BrandPaymentId,
				ChannelName = row.ChannelName,
				CreatedOn = row.CreatedOn,
				PaymentMethodId = row.PaymentMethodId,
				PaymentReturn = row.PaymentReturn,
				PlayerCode = row.PlayerCode,
				PromotionId = row.PromotionId,
				Remark = row.Remark,
				Status = row.Status,
				UpdatedOn = row.UpdatedOn,
				UserName = row.UserName,
				ThirdPartyOrderNo = row.ThirdPartyOrderNo,
				OrderNo = row.OrderNo,
				ReviewedUserName = row.ReviewedUserName,
				ReviewedOn = row.ReviewedOn,
				PromotionPlayerId = row.PromotionPlayerId
			});

		return new()
		{
			TotalCount = totalCount,
			Page = 1,
			Rows = -1,
			Items = items.ToEnumerable(),
			Sum = sum
		};
	}

	/// <summary>
	/// Reviews the deposit form asynchronous.
	/// </summary>
	/// <param name="signOffService">The signoff service.</param>
	/// <param name="formId">Deposit form id.</param>
	/// <param name="request">The request.</param>
	[HttpPost("{formId}", Name = "ReviewDepositForm")]
	public async Task ReviewDepositFormAsync(
		[FromServices] ICommandService<SignOffDepositFormCommand> signOffService,
		Guid formId,
		[FromBody] SignOffDepositFormViewModel request)
	{
		var command = new SignOffDepositFormCommand(
			formId,
			request.ReviewState,
			request.ReviewUserId,
			request.ReviewUserName,
			request.Remark);

		await signOffService.ExecuteAsync(command).ConfigureAwait(false);
	}

	/// <summary>
	/// Submits the deposit form status asynchronous.
	/// </summary>
	/// <param name="submitService">The submit service.</param>
	/// <param name="formId">Deposit form id.</param>
	/// <param name="request">The request.</param>
	[HttpPatch("{formId}", Name = "SubmitDepositFormStatus")]
	public async Task SubmitDepositFormStatusAsync(
		[FromServices] ICommandService<SubmitDepositFormStatusCommand> submitService,
		Guid formId,
		[FromBody] DepositFormStatusViewModel request)
	{
		var command = new SubmitDepositFormStatusCommand(
			formId,
			request.Status,
			request.PaymentReturn,
			request.ActualAmount,
			request.ThirdPartyOrderNo,
			request.PromotionPlayerId);

		await submitService.ExecuteAsync(command).ConfigureAwait(false);
	}

	/// <summary>
	/// Submits the deposit form third party order no asynchronous.
	/// </summary>
	/// <param name="submitService">The submit service.</param>
	/// <param name="formId">The form identifier.</param>
	/// <param name="request">The request.</param>
	/// <returns></returns>
	[HttpPatch("thirdparty/{formId}", Name = "SubmitDepositFormThirdPartyOrderNo")]
	[Obsolete("Method is obsolete, please use SubmitDepositFormStatus.")]
	public ValueTask SubmitDepositFormThirdPartyOrderNoAsync(
		[FromServices] ICommandService<SubmitDepositFormThirdPartyOrderNoCommand> submitService,
		Guid formId,
		[FromBody] DepositFormThirdPartyOrderNoViewModel request)
	{
		var command = new SubmitDepositFormThirdPartyOrderNoCommand(
			formId,
			request.ThirdPartyOrderNo);

		return submitService.ExecuteAsync(command);
	}

	/// <summary>
	/// Unlocks the payment asynchronous.
	/// </summary>
	/// <param name="unlockService">The unlock service.</param>
	/// <param name="viewModel">The view model.</param>
	/// <returns></returns>
	[HttpDelete("mutex", Name = "UnlockDepositPayment")]
	public ValueTask UnlockPaymentAsync(
		[FromServices] ICommandService<UnlockPaymentCommand> unlockService,
		[FromBody] LockDepositPaymentViewModel viewModel)
	{
		var command = new UnlockPaymentCommand(
			viewModel.PaymentCode,
			viewModel.PlayerCode);

		return unlockService.ExecuteAsync(command, HttpContext.RequestAborted);
	}
}
