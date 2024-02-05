using Valhalla.MessageQueue;

namespace Payment.IntegrationTests;

public class TestMessageProcessor<TMessage, TReply> : IMessageProcessor<TMessage, TReply>
{
	public virtual ValueTask<TReply> HandleAsync(
		string subject,
		TMessage data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
		=> ValueTask.FromResult(default(TReply)!);
}
