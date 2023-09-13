using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.Cache.MessageQueue;

internal class PaymentMethodCreatedEventHandler : IMessageHandler
{
	private readonly IPaymentMethodCacheCleaner _paymentMethodCacheCleaner;

	public PaymentMethodCreatedEventHandler(IPaymentMethodCacheCleaner paymentMethodCacheCleaner)
	{
		_paymentMethodCacheCleaner = paymentMethodCacheCleaner ?? throw new ArgumentNullException(nameof(paymentMethodCacheCleaner));
	}

	public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var eventData = PaymentMethodChangedEvent.Parser.ParseFrom(data.Span);

		_paymentMethodCacheCleaner.Clean(eventData.Name);

		return ValueTask.CompletedTask;
	}
}
