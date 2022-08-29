using System;

namespace Tgs.Payment.MessageQueue;

public static class ProcessorTypes
{
	public static readonly Type SaveWithdrawThirdNoHandlerType = typeof(SaveWithdrawThirdNoHandler);
	public static readonly Type TagWithdrawPaidHandlerType = typeof(TagWithdrawPaidHandler);
}
