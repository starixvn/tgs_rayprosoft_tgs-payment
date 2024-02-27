using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.MessageQueue;

internal class PaymentBalanceDispatchProcessor(
	HashSet<string> supportCodes,
	IMessageSender messageSender)
	: IMessageProcessor<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>
{
	private static readonly ReadOnlyMemory<byte> _NullResponse = new PaymentBalanceResponse().ToByteArray();

	public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(
		string subject,
		ReadOnlyMemory<byte> data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		// tgs.payment.dispatch-balance.code
		var paymentCode = subject.Split('.')[^1];

		return paymentCode switch
		{
			_ when supportCodes.Contains(paymentCode) => await messageSender.RequestAsync<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(
					$"tgs.payment.{paymentCode}.balance",
					data,
					cancellationToken).ConfigureAwait(false),
			_ => _NullResponse,
		};
	}
}
