using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.Cache.MessageQueue;

internal class PaymentMethodCreatedEventHandler(IPaymentMethodCacheCleaner paymentMethodCacheCleaner) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(
		string subject,
		ReadOnlyMemory<byte> data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		var eventData = PaymentMethodChangedEvent.Parser.ParseFrom(data.Span);

		paymentMethodCacheCleaner.Clean(eventData.Name);

		return ValueTask.CompletedTask;
	}
}
