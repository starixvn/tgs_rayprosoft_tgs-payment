using Json.Patch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tgs.Payment.ApplicationServices.Settings;
using Tgs.Payment.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Controllers;

[Route("setting")]
[ApiController]
public class SettingController : ControllerBase
{
	[HttpPut("player-bankinfo/disable/{brandId}", Name = "DisableCheckPlayerBankInfoDuplicate")]
	public ValueTask DisableCheckPlayerBankInfoDuplicateAsync(
		[FromServices] ICommandService<SetPlayerBankInfoDuplicateCheckIsEnabledCommand> configService,
		Guid brandId)
	{
		var command = new SetPlayerBankInfoDuplicateCheckIsEnabledCommand(brandId, false);

		return configService.ExecuteAsync(command, HttpContext.RequestAborted);
	}

	[HttpPut("player-bankinfo/enable/{brandId}", Name = "EnableCheckPlayerBankInfoDuplicate")]
	public ValueTask EnableCheckPlayerBankInfoDuplicateAsync(
		[FromServices] ICommandService<SetPlayerBankInfoDuplicateCheckIsEnabledCommand> configService,
		Guid brandId)
	{
		var command = new SetPlayerBankInfoDuplicateCheckIsEnabledCommand(brandId, true);

		return configService.ExecuteAsync(command, HttpContext.RequestAborted);
	}

	[HttpGet("player-bankinfo/{brandId}", Name = "GetPlayerBankInfoDuplicateCheckIsEnabled")]
	public ValueTask<bool> GetPlayerBankInfoDuplicateCheckIsEnabledAsync(
		[FromServices] IGetService<GetPlayerBankInfoDuplicateCheckIsEnabledQuery, bool> queryService,
		Guid brandId)
	{
		var query = new GetPlayerBankInfoDuplicateCheckIsEnabledQuery(brandId);

		return queryService.GetAsync(query, HttpContext.RequestAborted);
	}

	[HttpPatch("brand-payment/{brandId}", Name = "PatchBrandPaymentSettings")]
	public async Task PatchBrandPaymentSettingsAsync(
		[FromServices] IOptions<JsonOptions> jsonOptionsAccessor,
		[FromServices] ICommandService<UpsertBrandPaymentSettingsCommand> modifyService,
		Guid brandId,
		[FromBody] JsonPatch patch)
	{
		var viewModel = patch.Apply(
			BrandPaymentSettingsViewModel._Default,
			jsonOptionsAccessor.Value.JsonSerializerOptions);

		if (viewModel is null)
			return;

		var settings = new BrandPaymentSettings(viewModel.PlayerBankInfoCountLimit);

		var command = new UpsertBrandPaymentSettingsCommand(brandId, settings);

		await modifyService.ExecuteAsync(
			command,
			HttpContext.RequestAborted).ConfigureAwait(false);
	}
}
