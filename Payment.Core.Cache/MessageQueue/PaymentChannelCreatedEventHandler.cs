using Tgs.MessageQueue;
using Tgs.Payment.Protos;

namespace Tgs.Payment.Cache.MessageQueue;

internal class PaymentChannelCreatedEventHandler : IMessageHandler
{
	private readonly IPaymentChannelCacheCleaner _paymentChannelCacheCleaner;

	public PaymentChannelCreatedEventHandler(IPaymentChannelCacheCleaner paymentChannelCacheCleaner)
	{
		_paymentChannelCacheCleaner = paymentChannelCacheCleaner ?? throw new ArgumentNullException(nameof(paymentChannelCacheCleaner));
	}

	public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var eventData = PaymentChannelChangedEvent.Parser.ParseFrom(data.Span);

		_paymentChannelCacheCleaner.Clean(eventData.Name);

		return ValueTask.CompletedTask;
	}
}
