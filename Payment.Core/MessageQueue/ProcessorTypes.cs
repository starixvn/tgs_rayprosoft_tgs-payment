using System;

namespace Tgs.Payment.MessageQueue;

public static class ProcessorTypes
{
	public static Type SaveWithdrawThirdNoHandlerType = typeof(SaveWithdrawThirdNoHandler);
	public static Type TagWithdrawPaidHandlerType = typeof(TagWithdrawPaidHandler);
}
