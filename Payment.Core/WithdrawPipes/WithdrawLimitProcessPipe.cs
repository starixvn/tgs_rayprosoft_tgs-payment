using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.Payment.WithdrawLimit.Clients;
using Tgs.PlayerService.Client;
using Tgs.VipLevelService.Clients;

namespace Tgs.Payment.WithdrawPipes;

internal class WithdrawLimitProcessPipe(
	IVipLevelServiceClient vipLevelServiceClient,
	IPlayerServiceClient playerServiceClient,
	IWithdrawLimitClient withdrawLimitClient,
	IBrandServiceClient brandServiceClient,
	TimeProvider timeProvider) : ICreateWithdrawFormPipe
{
	public int Order => 5;

	public async ValueTask BeforeCreateWithdrawFormAsync(
		WithdrawUser user,
		Guid brandId,
		Guid bankInfoId,
		decimal amount,
		string? note,
		CancellationToken cancellationToken = default)
	{
		if (user.UserType is not UserType.Player)
			return;

		var brandInfo = await brandServiceClient.GetBrandInfoAsync(
			brandId,
			cancellationToken).ConfigureAwait(false);

		var brandTimeOffset = TimeSpan.FromMinutes(brandInfo!.TimeSpanMinutes ?? 8 * 60);

		var brandVipLevels = await vipLevelServiceClient.GetBrandAllVipLevelsAsync(brandId, cancellationToken)
			.OrderBy(vip => vip.Level)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		var playerInfo = await playerServiceClient.GetPlayerInfoAsync(
			user.UserId!,
			cancellationToken).ConfigureAwait(false);

		var vipInfo = brandVipLevels.Where(vip => vip.Id == playerInfo!.VipLevelId)
			.FirstOrDefault() ?? brandVipLevels[0];

		var currentDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().ToOffset(brandTimeOffset).DateTime);

		var result = await withdrawLimitClient.IncreasePlayerWithdrawStatisticsAsync(
			user.UserId!,
			currentDate,
			amount,
			cancellationToken).ConfigureAwait(false);

		var errors = new Dictionary<string, object>();

		if (vipInfo.WithdrawTotalAmountPerDay >= 0M && result.TotalAmount > vipInfo.WithdrawTotalAmountPerDay)
			errors.Add("Amount", result.TotalAmount);

		if (vipInfo.WithdrawTransactionsPerDay >= 0M && result.Count > vipInfo.WithdrawTransactionsPerDay)
			errors.Add("Count", result.Count);

		if (errors.Count > 0)
		{
			_ = await withdrawLimitClient.DecreasePlayerWithdrawStatisticsAsync(
				user.UserId!,
				currentDate,
				amount,
				cancellationToken).ConfigureAwait(false);

			throw new WithdrawLimitExceedException(errors);
		}
	}
}
