using JSLTSharp;

namespace Tgs.Payment.WithdrawProcessors;

internal class WinPayProcessor(JsonTransform jsonTransform) : PhpGoPayProcessor(jsonTransform)
{
	public override string Subject => "tgs.payment.winpay.withdraw";
}
