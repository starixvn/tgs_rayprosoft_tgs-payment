using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices.Deposits;
using Tgs.Payment.Controllers.Constraints;
using Tgs.Payment.ViewModels;
using TGS.Paging;
using TGS.ServiceUtility;

namespace Tgs.Payment.Controllers
{
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
		[HttpPut("{createFormId}")]
		public async Task<Guid> CreateDepositFormAsync(
			[FromServices] ICommandService<CreateDepositFormCommand> createService,
			Guid createFormId,
			[FromBody] CreateDepositFormViewModel request)
		{
			var createInfo = new DepositFormCreateInfo(
				request.PlayerCode,
				request.BrandId,
				request.BrandPaymentId,
				request.Amount,
				request.PromotionId,
				request.ApplyInfo);

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
		[HttpGet("{brandId}/{formId}")]
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
					ThirdPartyOrderNo = formData.ThirdPartyOrderNo
				}
				: null;
		}

		[HttpGet("{formId}", Order = 1), HttpHeaderConstraint("FormQuery")]
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
					ThirdPartyOrderNo = formData.ThirdPartyOrderNo
				}
				: null;
		}

		/// <summary>
		/// Queries the deposit forms asynchronous.
		/// </summary>
		/// <param name="countService">The count service.</param>
		/// <param name="queryService">The query service.</param>
		/// <param name="brandId">The brand identifier.</param>
		/// <param name="formQuery">The form query.</param>
		/// <param name="paging">The paging.</param>
		/// <returns></returns>
		[HttpGet("{brandId}", Order = 10)]
		public async Task<PagingResponseViewModel<DepositFormViewModel>> QueryDepositFormsAsync(
			[FromServices] IGetService<DepositFormQuery, int> countService,
			[FromServices] IQueryService<DepositFormQuery, DepositFormInfo> queryService,
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

			var queryPaging = new Paging<DepositFormInfo>();

			if (paging.ShowAll == true)
			{
				var query = new DepositFormQuery(
					brandId,
					formQuery.UserName,
					formQuery.ChannelName,
					formQuery.ThirdPartyOrderNo,
					formQuery.FormId,
					formQuery.PaymentMethodId,
					formQuery.Status ?? Array.Empty<PaymentFlowState>(),
					createOnRange,
					updateOnRange,
					null);

				var totalCount = await countService.GetAsync(query).ConfigureAwait(false);

				var items = queryService.QueryAsync(query)
					.Select(row => new DepositFormViewModel
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
						PlayerCode = row.PlayerCode,
						ThirdPartyOrderNo = row.ThirdPartyOrderNo,
						PromotionId = row.PromotionId,
						Remark = row.Remark,
						Status = row.Status,
						UpdatedOn = row.UpdatedOn,
						UserName = row.UserName
					});

				return new PagingResponseViewModel<DepositFormViewModel>
				{
					TotalCount = totalCount,
					Page = 1,
					Rows = -1,
					Items = items.ToEnumerable()
				};
			}
			else
			{
				var sourcePaging = new Paging<DepositFormInfo>();

				sourcePaging.Page = paging.Page ?? 1;
				sourcePaging.RowCount = paging.Rows ?? 10;

				if (!string.IsNullOrWhiteSpace(paging.SortBy))
					sourcePaging.SortByPropertyName(paging.SortBy, paging.IsDesc ?? false);

				var query = new DepositFormQuery(
					brandId,
					formQuery.UserName,
					formQuery.ChannelName,
					formQuery.ThirdPartyOrderNo,
					formQuery.FormId,
					formQuery.PaymentMethodId,
					formQuery.Status ?? Array.Empty<PaymentFlowState>(),
					createOnRange,
					updateOnRange,
					sourcePaging);

				var totalCount = await countService.GetAsync(query).ConfigureAwait(false);

				var items = queryService.QueryAsync(query)
					.Select(row => new DepositFormViewModel
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
						PlayerCode = row.PlayerCode,
						PromotionId = row.PromotionId,
						Remark = row.Remark,
						Status = row.Status,
						UpdatedOn = row.UpdatedOn,
						UserName = row.UserName,
						ThirdPartyOrderNo = row.ThirdPartyOrderNo
					});

				return new PagingResponseViewModel<DepositFormViewModel>
				{
					TotalCount = totalCount,
					Page = 1,
					Rows = -1,
					Items = items.ToEnumerable()
				};
			}
		}

		/// <summary>
		/// Reviews the deposit form asynchronous.
		/// </summary>
		/// <param name="signOffService">The signoff service.</param>
		/// <param name="formId">Deposit form id.</param>
		/// <param name="request">The request.</param>
		[HttpPost("{formId}")]
		public async Task ReviewDepositFormAsync(
			[FromServices] ICommandService<SignOffDepositFormCommand> signOffService,
			Guid formId,
			[FromBody] SignOffDepositFormViewModel request)
		{
			var command = new SignOffDepositFormCommand(
				formId,
				request.ReviewState,
				request.Remark);

			await signOffService.ExecuteAsync(command).ConfigureAwait(false);
		}

		/// <summary>
		/// Submits the deposit form status asynchronous.
		/// </summary>
		/// <param name="submitService">The submit service.</param>
		/// <param name="formId">Deposit form id.</param>
		/// <param name="request">The request.</param>
		[HttpPatch("{formId}")]
		public async Task SubmitDepositFormStatusAsync(
			[FromServices] ICommandService<SubmitDepositFormStatusCommand> submitService,
			Guid formId,
			[FromBody] DepositFormStatusViewModel request)
		{
			var command = new SubmitDepositFormStatusCommand(
				formId,
				request.Status,
				request.PaymentReturn);

			await submitService.ExecuteAsync(command).ConfigureAwait(false);
		}

		[HttpPatch("thirdparty/{formId}")]
		public async Task SubmitDepositFormThirdPartyOrderNoAsync(
			[FromServices] ICommandService<SubmitDepositFormThirdPartyOrderNoCommand> submitService,
			Guid formId,
			[FromBody] DepositFormThirdPartyOrderNoViewModel request)
		{
			var command = new SubmitDepositFormThirdPartyOrderNoCommand(
				formId,
				request.ThirdPartyOrderNo);

			await submitService.ExecuteAsync(command).ConfigureAwait(false);
		}
	}
}
