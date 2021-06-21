using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApplicationServices;
using Tgs.Payment.ViewModels;
using TGS.ServiceUtility;

namespace Tgs.Payment.Controllers
{
	[Route("bankinfo/player")]
	[ApiController]
	public class PlayerBankInfoController : ControllerBase
	{
		[HttpGet("{playerCode}")]
		public IAsyncEnumerable<PlayerBankInfoViewModel> GetPlayerBankInfosAsync(
			[FromServices] IQueryService<GetPlayerBankInfosQuery, BankInfo> queryService,
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
