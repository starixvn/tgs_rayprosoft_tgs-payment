using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.Cache.MessageQueue;

internal class PaymentMethodChannelChangedEventHandler(
	IPaymentMethodChannelCacheCleaner paymentMethodChannelCacheCleaner)
	: IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(
		string subject,
		ReadOnlyMemory<byte> data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		var eventData = PaymentMethodChannelChangedEvent.Parser.ParseFrom(data.Span);

		paymentMethodChannelCacheCleaner.Clean(eventData.Id);

		return ValueTask.CompletedTask;
	}
}
