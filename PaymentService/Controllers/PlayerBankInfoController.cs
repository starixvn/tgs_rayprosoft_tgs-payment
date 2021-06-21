using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tgs.Payment.ApplicationServices;
using Tgs.Payment.ViewModels;
using TGS.ServiceUtility;

namespace Tgs.Payment.Controllers
{
	[Route("bankinfo/player")]
	[ApiController]
	public class PlayerBankInfoController : ControllerBase
	{
		[HttpPut("{brandId}/{playerCode}")]
		public async Task<Guid> CreatePlayerBankInfoAsync(
			[FromServices] ICommandService<CreatePlayerBankInfoCommand> createService,
			[FromServices] IOptions<JsonOptions> jsonOptions,
			Guid brandId,
			string playerCode,
			[FromBody] CreateBankInfoViewModel request)
		{
			var command = new CreatePlayerBankInfoCommand(
				brandId,
				playerCode,
				request.PaymentChannelId,
				request.BankType,
				JsonSerializer.Serialize(request.Detail, jsonOptions.Value.JsonSerializerOptions));

			await createService.ExecuteAsync(command).ConfigureAwait(false);

			return await command.TaskSource.Task.ConfigureAwait(false);
		}

		[HttpGet("{playerCode}")]
		public IAsyncEnumerable<PlayerBankInfoViewModel> GetPlayerBankInfosAsync(
			[FromServices] IQueryService<GetPlayerBankInfosQuery, PlayerBankInfo> queryService,
			string playerCode)
		{
			var query = new GetPlayerBankInfosQuery(playerCode);

			return queryService.QueryAsync(query)
				.Select(bankInfo => new PlayerBankInfoViewModel
				{
					Id = bankInfo.Id,
					PlayerCode = bankInfo.PlayerCode,
					BankType = bankInfo.BankType,
					BrandId = bankInfo.BrandId,
					PaymentChannelId = bankInfo.PaymentChannelId,
					Detail = JsonDocument.Parse(bankInfo.Detail)
				});
		}
	}
}
