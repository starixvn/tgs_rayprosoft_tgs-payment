using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;

namespace Tgs.Payment.WithdrawPipes;

internal class EmitCreateWithdrawFormEventPipe(IPaymentClient paymentClient) : ICreateWithdrawFormPipe
{
	public int Order { get; } = 0;

	public ValueTask AfterCreateWithdrawFormAsync(
		WithdrawUser user,
		Guid brandId,
		Guid bankInfoId,
		decimal amount,
		string? note,
		Guid createFormId,
		CancellationToken cancellationToken = default)
		=> paymentClient.EmitWithdrawFormCreatedEventAsync(
			createFormId,
			brandId,
			cancellationToken);
}
