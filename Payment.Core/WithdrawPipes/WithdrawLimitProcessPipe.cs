using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.WithdrawLimit.Clients;
using Tgs.PlayerService.Client;
using Tgs.VipLevelService.Clients;

namespace Tgs.Payment.WithdrawPipes;

internal class WithdrawLimitProcessPipe : ICreateWithdrawFormPipe
{
	private readonly IPlayerServiceClient _playerServiceClient;
	private readonly ISystemClock _systemClock;
	private readonly IVipLevelServiceClient _vipLevelServiceClient;
	private readonly IWithdrawLimitClient _withdrawLimitClient;
	public int Order => 5;

	public WithdrawLimitProcessPipe(
		IVipLevelServiceClient vipLevelServiceClient,
		IPlayerServiceClient playerServiceClient,
		IWithdrawLimitClient withdrawLimitClient,
		ISystemClock systemClock)
	{
		_vipLevelServiceClient = vipLevelServiceClient ?? throw new ArgumentNullException(nameof(vipLevelServiceClient));
		_playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
		_withdrawLimitClient = withdrawLimitClient ?? throw new ArgumentNullException(nameof(withdrawLimitClient));
		_systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
	}

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

		var brandVipLevels = await _vipLevelServiceClient.GetBrandAllVipLevelsAsync(brandId, cancellationToken)
			.OrderBy(vip => vip.Level)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		var playerInfo = await _playerServiceClient.GetPlayerInfoAsync(
			user.UserId!,
			cancellationToken).ConfigureAwait(false);

		var vipInfo = brandVipLevels.Where(vip => vip.Id == playerInfo!.VipLevelId)
			.FirstOrDefault() ?? brandVipLevels[0];

		var currentDate = DateOnly.FromDateTime(_systemClock.UtcNow.ToOffset(TimeSpan.FromHours(8D)).DateTime);

		var result = await _withdrawLimitClient.IncreasePlayerWithdrawStatisticsAsync(
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
			_ = await _withdrawLimitClient.DecreasePlayerWithdrawStatisticsAsync(
				user.UserId!,
				currentDate,
				amount,
				cancellationToken).ConfigureAwait(false);

			throw new WithdrawLimitExceedException(errors);
		}
	}
}
