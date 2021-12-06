using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.Payment.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class PaymentController : ControllerBase
	{
		[HttpPut]
		public ValueTask CreatePaymentAsync(
			[FromServices] ICommandService<CreatePaymentCommand> createService,
			[FromServices] IOptions<JsonOptions> jsonOptionsAccessor,
			[FromBody] PaymentCreateViewModel request)
		{
			var jsonOptions = jsonOptionsAccessor.Value;

			var command = new CreatePaymentCommand(
				request.Code,
				request.Description,
				JsonSerializer.Serialize(
					request.ExtendedData,
					jsonOptions.JsonSerializerOptions));

			return createService.ExecuteAsync(command);
		}

		[HttpGet]
		public IAsyncEnumerable<PaymentViewModel> GetAllPaymentsAsync(
			[FromServices] IQueryService<GetAllPaymentsQuery, PaymentInfo> queryService)
		{
			var allPayments = queryService.QueryAsync(new GetAllPaymentsQuery());

			return allPayments.Select(payment => new PaymentViewModel
			{
				Id = payment.Id,
				Code = payment.Code
			});
		}

		[HttpGet("{id}")]
		public async Task<PaymentViewModel> GetPaymentByIdAsync(
			[FromServices] IGetService<GetPaymentQuery, PaymentInfo> getService,
			Guid id)
		{
			var query = new GetPaymentQuery(id);

			var info = await getService.GetAsync(query).ConfigureAwait(false);

			return new PaymentViewModel
			{
				Id = info.Id,
				Code = info.Code
			};
		}
	}
}
