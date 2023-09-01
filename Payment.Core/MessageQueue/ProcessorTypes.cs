using System;

namespace Tgs.Payment.MessageQueue;

public static class ProcessorTypes
{
	public static readonly Type ApproveWithdrawHandlerType = typeof(AutoApproveWithdrawHandler);
	public static readonly Type CountBrandDepositApprovedType = typeof(CountBrandDepositApprovedHandler);
	public static readonly Type CountBrandWithdrawPaidType = typeof(CountBrandWithdrawPaidHandler);
	public static readonly Type PlayerBankInfoAddedHandlerType = typeof(PlayerBankInfoAddedHandler);
	public static readonly Type PlayerBankInfoDeletedHandlerType = typeof(PlayerBankInfoDeletedHandler);
	public static readonly Type RebuildPlayerBankInfoTagHandlerType = typeof(RebuildPlayerBankInfoHashHandler);
	public static readonly Type SaveWithdrawThirdNoHandlerType = typeof(SaveWithdrawThirdNoHandler);
	public static readonly Type TagWithdrawPaidHandlerType = typeof(TagWithdrawPaidHandler);
}
