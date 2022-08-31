using Microsoft.AspNetCore.Mvc;
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
		[FromServices] IGetService<CreatePlayerBankInfoCommand, Guid> createService,
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
			request.Detail,
			request.IsDefault ?? false,
			modifyUser ?? playerCode);

		return await createService
			.GetAsync(command, HttpContext.RequestAborted)
			.ConfigureAwait(false);
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
				Detail = bankInfo.Detail,
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
