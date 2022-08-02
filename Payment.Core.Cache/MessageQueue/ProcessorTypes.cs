namespace Tgs.Payment.Cache.MessageQueue;

public static class ProcessorTypes
{
	public static readonly Type PaymentChannelCreatedEventHandlerType = typeof(PaymentChannelCreatedEventHandler);
	public static readonly Type PaymentCreatedEventHandlerType = typeof(PaymentCreatedEventHandler);
	public static readonly Type PaymentMethodChannelChangedEventHandlerType = typeof(PaymentMethodChannelChangedEventHandler);
	public static readonly Type PaymentMethodCreatedEventHandlerType = typeof(PaymentMethodCreatedEventHandler);
}
