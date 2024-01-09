using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.MessageQueue;

internal class PlayerBankInfoDeletedHandler(IPaymentRepository paymentRepository) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(
		string subject,
		ReadOnlyMemory<byte> data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		var request = PlayerBankInfoChangedEvent.Parser.ParseFrom(data.Span);

		return paymentRepository.RemoveBankInfoTagsAsync(request.BrandId, request.Id, cancellationToken);
	}
}
