using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices.BankInfos;
using Tgs.Payment.ViewModels;
using Tgs.ServiceUtility;
using Tgs.WebApi.Toolkits;

namespace Tgs.Payment.Controllers;

[Route("bankinfo/player")]
[ApiController]
public class PlayerBankInfoController : ControllerBase
{
	[SuccessStatusCode(StatusCodes.Status200OK)]
	[FailStatusCode(StatusCodes.Status409Conflict, "see response content.")]
	[HttpPut("{brandId}/{playerCode}", Name = "CreatePlayerBankInfo")]
	public async Task<Guid> CreatePlayerBankInfoAsync(
		[FromServices] IGetService<CreatePlayerBankInfoCommand, Guid> createService,
		[FromHeader(Name = "ModifyUser")] string? modifyUser,
		Guid brandId,
		string playerCode,
		[FromBody] CreateBankInfoViewModel request)
	{
		try
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
		catch (PlayerBankInfoDuplicateException)
		{
			return HttpContext.TransformResult<Guid>(Problem(
				statusCode: 409,
				title: nameof(PlayerBankInfoDuplicateException),
				detail: "Can't create player bankinfo."));
		}
		catch (PlayerBankInfoCountReachedLimitException)
		{
			return HttpContext.TransformResult<Guid>(Problem(
				statusCode: 409,
				title: nameof(PlayerBankInfoCountReachedLimitException)));
		}
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

		return queryService.QueryAsync(query, HttpContext.RequestAborted)
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

	[HttpGet("query/{tagType}/{brandId}", Name = "GetPlayersByBankInfoTag")]
	public IAsyncEnumerable<string> GetPlayersByBankInfoTagAsync(
		[FromServices] IQueryService<GetPlayersByBankInfoTagQuery, string> queryService,
		string tagType,
		Guid brandId,
		[FromQuery] string tag)
	{
		var query = new GetPlayersByBankInfoTagQuery(brandId, tagType, tag);

		return queryService.QueryAsync(query, HttpContext.RequestAborted);
	}

	[HttpPost("tag/{id}")]
	public ValueTask SetBankInfoTagsAsync(
		[FromServices] ICommandService<ProcessBankInfoTagsCommand> tagService,
		Guid id)
	{
		var command = new ProcessBankInfoTagsCommand(id);

		return tagService.ExecuteAsync(command, HttpContext.RequestAborted);
	}

	[HttpPatch("default/{playerCode}/{id}", Name = "SetDefaultPlayerBankInfo")]
	[HttpPatch("{playerCode}/{id}/default")]
	public ValueTask SetDefaultPlayerBankInfoAsync(
		[FromServices] ICommandService<SetDefaultPlayerBankInfoCommand> setService,
		[FromHeader(Name = "ModifyUser")] string modifyUser,
		string playerCode,
		Guid id)
		=> setService.ExecuteAsync(
			new SetDefaultPlayerBankInfoCommand(playerCode, id, modifyUser),
			HttpContext.RequestAborted);
}
