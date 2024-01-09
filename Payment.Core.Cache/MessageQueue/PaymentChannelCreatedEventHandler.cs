using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.Cache.MessageQueue;

internal class PaymentChannelCreatedEventHandler(IPaymentChannelCacheCleaner paymentChannelCacheCleaner) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(
		string subject,
		ReadOnlyMemory<byte> data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		var eventData = PaymentChannelChangedEvent.Parser.ParseFrom(data.Span);

		paymentChannelCacheCleaner.Clean(eventData.Name);

		return ValueTask.CompletedTask;
	}
}
