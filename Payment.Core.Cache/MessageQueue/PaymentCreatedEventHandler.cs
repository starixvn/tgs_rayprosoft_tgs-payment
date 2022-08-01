using Tgs.MessageQueue;
using Tgs.Payment.Protos;

namespace Tgs.Payment.Cache.MessageQueue;

internal class PaymentCreatedEventHandler : IMessageHandler
{
	private readonly IPaymentCacheCleaner _paymentCacheCleaner;

	public PaymentCreatedEventHandler(IPaymentCacheCleaner paymentCacheCleaner)
	{
		_paymentCacheCleaner = paymentCacheCleaner ?? throw new ArgumentNullException(nameof(paymentCacheCleaner));
	}

	public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var eventData = PaymentChangedEvent.Parser.ParseFrom(data.Span);

		_paymentCacheCleaner.Clean(eventData.Code);

		return ValueTask.CompletedTask;
	}
}
