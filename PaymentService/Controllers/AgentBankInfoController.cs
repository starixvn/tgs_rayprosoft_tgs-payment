using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tgs.Payment.ApplicationServices.BankInfos;
using Tgs.Payment.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Controllers
{
	[Route("bankinfo/agent")]
	[ApiController]
	public class AgentBankInfoController : ControllerBase
	{
		[HttpPut("{brandId}/{agentId}")]
		public async Task<Guid> CreateAgentBankInfoAsync(
			[FromServices] ICommandService<CreateAgentBankInfoCommand> createService,
			[FromServices] IOptions<JsonOptions> jsonOptionsAccessor,
			Guid brandId,
			Guid agentId,
			[FromQuery(Name = "name")] string agentName,
			[FromBody] CreateBankInfoViewModel request)
		{
			var command = new CreateAgentBankInfoCommand(
				brandId,
				agentId,
				agentName,
				request.PaymentChannelId,
				request.BankType,
				JsonSerializer.Serialize(request.Detail, jsonOptionsAccessor.Value.JsonSerializerOptions));

			await createService.ExecuteAsync(command).ConfigureAwait(false);

			return await command.TaskSource.Task.ConfigureAwait(false);
		}

		[HttpDelete("{agentId}/{formId}")]
		public ValueTask DeleteAgentBankInfoAsync(
			[FromServices] ICommandService<DeleteAgentBankInfoCommand> deleteService,
			Guid agentId,
			Guid formId)
			=> deleteService.ExecuteAsync(new DeleteAgentBankInfoCommand(agentId, formId));

		[HttpGet("{agentId}")]
		public IAsyncEnumerable<AgentBankInfoViewModel> GetAgentBankInfosAsync(
			[FromServices] IQueryService<GetAgentBankInfosQuery, AgentBankInfo> queryService,
			Guid agentId)
		{
			var query = new GetAgentBankInfosQuery(agentId);

			return queryService.QueryAsync(query)
				.Select(bankInfo => new AgentBankInfoViewModel
				{
					Id = bankInfo.Id,
					AgentId = bankInfo.AgentId,
					BankType = bankInfo.BankType,
					BrandId = bankInfo.BrandId,
					PaymentChannelId = bankInfo.PaymentChannelId,
					Detail = JsonDocument.Parse(bankInfo.Detail)
				});
		}
	}
}
