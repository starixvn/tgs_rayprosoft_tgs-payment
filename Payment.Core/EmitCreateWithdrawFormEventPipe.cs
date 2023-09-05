using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;

namespace Tgs.Payment;

internal class EmitCreateWithdrawFormEventPipe : ICreateWithdrawFormPipe
{
	private readonly IPaymentClient _paymentClient;

	public int Order { get; } = 0;

	public EmitCreateWithdrawFormEventPipe(IPaymentClient paymentClient)
	{
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
	}

	public ValueTask AfterCreateWithdrawFormAsync(
		WithdrawUser user,
		Guid brandId,
		Guid bankInfoId,
		decimal amount,
		string? note,
		Guid createFormId,
		CancellationToken cancellationToken = default)
		=> _paymentClient.EmitWithdrawFormCreatedEventAsync(
			createFormId,
			brandId,
			cancellationToken);
}
