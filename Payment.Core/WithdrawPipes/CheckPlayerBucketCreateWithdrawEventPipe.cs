using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerBucket.Client;
using Tgs.Service.PlayerBuckets;

namespace Tgs.Payment.WithdrawPipes;

internal class CheckPlayerBucketCreateWithdrawEventPipe : ICreateWithdrawFormPipe
{
	private readonly IPlayerBucketClient _playerBucketClient;
	private readonly IPlayerBucketForTypesClient _playerBucketForTypesClient;

	public int Order => 10;

	public CheckPlayerBucketCreateWithdrawEventPipe(
		IPlayerBucketClient playerBucketClient,
		IPlayerBucketForTypesClient playerBucketForTypesClient)
	{
		_playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
		_playerBucketForTypesClient = playerBucketForTypesClient ?? throw new ArgumentNullException(nameof(playerBucketForTypesClient));
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

		var bucketTypes = _playerBucketClient.GetBrandBucketTypesAsync(brandId, cancellationToken);

		var bucketNotEmpty = await _playerBucketForTypesClient.ReadBucketsForBucketTypesAsync(
			user.UserId!,
			bucketTypes.ToEnumerable(),
			cancellationToken)
			.AnyAsync(
				result => result.Result.Amount > 0M,
				cancellationToken)
			.ConfigureAwait(false);

		if (bucketNotEmpty)
			throw new InsufficientBucketException();
	}
}
