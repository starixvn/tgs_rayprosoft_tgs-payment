using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.Cache.MessageQueue;

internal class PaymentMethodChannelChangedEventHandler : IMessageHandler
{
	private readonly IPaymentMethodChannelCacheCleaner _paymentMethodChannelCacheCleaner;

	public PaymentMethodChannelChangedEventHandler(IPaymentMethodChannelCacheCleaner paymentMethodChannelCacheCleaner)
	{
		_paymentMethodChannelCacheCleaner = paymentMethodChannelCacheCleaner ?? throw new ArgumentNullException(nameof(paymentMethodChannelCacheCleaner));
	}

	public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var eventData = PaymentMethodChannelChangedEvent.Parser.ParseFrom(data.Span);

		_paymentMethodChannelCacheCleaner.Clean(eventData.Id);

		return ValueTask.CompletedTask;
	}
}
