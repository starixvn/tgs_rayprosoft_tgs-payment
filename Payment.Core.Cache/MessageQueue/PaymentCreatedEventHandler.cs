using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.Cache.MessageQueue;

internal class PaymentCreatedEventHandler(IPaymentCacheCleaner paymentCacheCleaner) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(
		string subject,
		ReadOnlyMemory<byte> data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		var eventData = PaymentChangedEvent.Parser.ParseFrom(data.Span);

		paymentCacheCleaner.Clean(eventData.Code);

		return ValueTask.CompletedTask;
	}
}
