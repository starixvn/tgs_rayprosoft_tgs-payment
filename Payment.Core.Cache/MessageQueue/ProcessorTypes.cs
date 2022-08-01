namespace Tgs.Payment.Cache.MessageQueue;

public static class ProcessorTypes
{
	public static readonly Type PaymentCreatedEventHandlerType = typeof(PaymentCreatedEventHandler);
	public static readonly Type PaymentMethodCreatedEventHandlerType = typeof(PaymentMethodCreatedEventHandler);
}
