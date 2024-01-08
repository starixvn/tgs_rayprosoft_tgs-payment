using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerBucket.Client;
using Tgs.Service.PlayerBuckets;

namespace Tgs.Payment.WithdrawPipes;

internal class CheckPlayerBucketCreateWithdrawEventPipe(
	IPlayerBucketClient playerBucketClient,
	IPlayerBucketForTypesClient playerBucketForTypesClient) : ICreateWithdrawFormPipe
{
	public int Order => 10;

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

		var bucketTypes = playerBucketClient.GetBrandBucketTypesAsync(brandId, cancellationToken);

		var bucketNotEmpty = await playerBucketForTypesClient.ReadBucketsForBucketTypesAsync(
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
