using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tgs.Payment.ApplicationServices.BankInfos;
using Tgs.Payment.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Controllers;

[Route("bankinfo/player")]
[ApiController]
public class PlayerBankInfoController : ControllerBase
{
	[HttpPut("{brandId}/{playerCode}", Name = "CreatePlayerBankInfo")]
	public async Task<Guid> CreatePlayerBankInfoAsync(
		[FromServices] ICommandService<CreatePlayerBankInfoCommand> createService,
		[FromServices] IOptions<JsonOptions> jsonOptionsAccessor,
		[FromHeader(Name = "ModifyUser")] string? modifyUser,
		Guid brandId,
		string playerCode,
		[FromBody] CreateBankInfoViewModel request)
	{
		var command = new CreatePlayerBankInfoCommand(
			brandId,
			playerCode,
			request.PaymentChannelId,
			request.BankType,
			JsonSerializer.Serialize(request.Detail, jsonOptionsAccessor.Value.JsonSerializerOptions),
			request.IsDefault ?? false,
			modifyUser ?? playerCode);

		await createService.ExecuteAsync(command).ConfigureAwait(false);

		return await command.TaskSource.Task.ConfigureAwait(false);
	}

	[HttpDelete("{playerCode}/{id}", Name = "DeletePlayerBankInfo")]
	public ValueTask DeletePlayerBankInfoAsync(
		[FromServices] ICommandService<DeletePlayerBankInfoCommand> deleteService,
		string playerCode,
		Guid id)
		=> deleteService.ExecuteAsync(
			new DeletePlayerBankInfoCommand(playerCode, id));

	[HttpGet("{playerCode}", Name = "QueryPlayerBankInfos")]
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
				Detail = JsonDocument.Parse(bankInfo.Detail),
				IsDefault = bankInfo.IsDefault
			});
	}

	[HttpPatch("default/{playerCode}/{id}", Name = "SetDefaultPlayerBankInfo")]
	[HttpPatch("{playerCode}/{id}/default")]
	public ValueTask SetDefaultPlayerBankInfoAsync(
		[FromServices] ICommandService<SetDefaultPlayerBankInfoCommand> setService,
		[FromHeader(Name = "ModifyUser")] string modifyUser,
		string playerCode,
		Guid id)
		=> setService.ExecuteAsync(
			new SetDefaultPlayerBankInfoCommand(playerCode, id, modifyUser));
}
